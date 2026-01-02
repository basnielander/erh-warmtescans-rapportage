import { Injectable, signal, computed, effect } from '@angular/core';
import { environment } from '../../environments/environment';

/// <reference types="gapi" />
declare const gapi: any;
declare const google: any;

export interface User {
  id: string;
  name: string;
  email: string;
  picture: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  // Session storage keys
  private readonly ACCESS_TOKEN_KEY = 'google_access_token';
  private readonly USER_KEY = 'google_user';

  // Signals for state management
  private userSignal = signal<User | null>(null);
  private isLoadingSignal = signal<boolean>(false);
  private errorSignal = signal<string | null>(null);
  private isAuthInitializedSignal = signal<boolean>(false);

  // Public computed signals
  user = this.userSignal.asReadonly();
  isLoading = this.isLoadingSignal.asReadonly();
  error = this.errorSignal.asReadonly();
  isAuthenticated = computed(() => this.userSignal() !== null);
  isAuthInitialized = this.isAuthInitializedSignal.asReadonly();

  private tokenClient: any = null;
  private accessToken: string | null = null;

  constructor() {
    // Effect to log authentication state changes
    effect(() => {
      const user = this.userSignal();
    });

    // Restore authentication state from session storage
    this.restoreAuthState();
  }

  /**
   * Restore authentication state from session storage
   */
  private restoreAuthState(): void {
    try {
      const storedToken = localStorage.getItem(this.ACCESS_TOKEN_KEY);
      const storedUser = localStorage.getItem(this.USER_KEY);

      if (storedToken && storedUser) {
        this.accessToken = storedToken;
        this.userSignal.set(JSON.parse(storedUser));
      }
    } catch (error) {
      console.error('Failed to restore auth state:', error);
      this.clearLocalStorage();
    }
  }

  /**
   * Save access token to session storage
   */
  private saveAccessToken(token: string): void {
    try {
      localStorage.setItem(this.ACCESS_TOKEN_KEY, token);
      this.accessToken = token;
    } catch (error) {
      console.error('Failed to save access token:', error);
    }
  }

  /**
   * Save user to session storage
   */
  private saveUser(user: User): void {
    try {
      localStorage.setItem(this.USER_KEY, JSON.stringify(user));
      this.userSignal.set(user);
    } catch (error) {
      console.error('Failed to save user:', error);
    }
  }

  /**
   * Clear session storage
   */
  private clearLocalStorage(): void {
    try {
      localStorage.removeItem(this.ACCESS_TOKEN_KEY);
      localStorage.removeItem(this.USER_KEY);
    } catch (error) {
      console.error('Failed to clear local storage:', error);
    }
  }

  /**
   * Initialize Google Auth with new Identity Services
   */
  async initAuth(): Promise<void> {
    if (this.isAuthInitializedSignal()) {
      return;
    }

    this.isLoadingSignal.set(true);
    this.errorSignal.set(null);

    try {
      await this.loadGapiClient();
      await this.initGapiClient();
      this.initTokenClient();
      this.isAuthInitializedSignal.set(true);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to initialize authentication';
      this.errorSignal.set(errorMessage);
      console.error('Auth initialization error:', error);
    } finally {
      this.isLoadingSignal.set(false);
    }
  }

  /**
   * Load Google API client library
   */
  private loadGapiClient(): Promise<void> {
    return new Promise((resolve, reject) => {
      if (typeof gapi === 'undefined') {
        reject(new Error('Google API not loaded'));
        return;
      }

      gapi.load('client', {
        callback: () => resolve(),
        onerror: () => reject(new Error('Failed to load Google API'))
      });
    });
  }

  /**
   * Initialize GAPI client for Drive API
   */
  private async initGapiClient(): Promise<void> {
    try {
      await gapi.client.init({
        discoveryDocs: ['https://www.googleapis.com/discovery/v1/apis/drive/v3/rest']
      });
    } catch (error) {
      throw new Error('Failed to initialize Google API Client');
    }
  }

  /**
   * Initialize Google Identity Services token client
   */
  private initTokenClient(): void {
    if (typeof google === 'undefined') {
      throw new Error('Google Identity Services not loaded');
    }

    this.tokenClient = google.accounts.oauth2.initTokenClient({
      client_id: environment.googleClientId,
      scope: environment.scopes.join(' '),
      callback: (response: any) => {
        if (response.error) {
          this.errorSignal.set(response.error);
          console.error('Token error:', response);
          return;
        }
        this.saveAccessToken(response.access_token);
        this.fetchUserInfo();
      },
    });
  }

  /**
   * Sign in with Google - Request access token
   */
  async signIn(): Promise<void> {
    if (!this.tokenClient) {
      this.errorSignal.set('Authentication not initialized');
      return;
    }

    this.isLoadingSignal.set(true);
    this.errorSignal.set(null);

    try {
      // Request an access token
      this.tokenClient.requestAccessToken({ prompt: 'consent' });
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to sign in';
      this.errorSignal.set(errorMessage);
      console.error('Sign in error:', error);
      this.isLoadingSignal.set(false);
    }
  }

  /**
   * Fetch user info from Google
   */
  private async fetchUserInfo(): Promise<void> {
    try {
      const response = await fetch('https://www.googleapis.com/oauth2/v2/userinfo', {
        headers: {
          Authorization: `Bearer ${this.accessToken}`
        }
      });

      if (!response.ok) {
        throw new Error('Failed to fetch user info');
      }

      const data = await response.json();

      const user: User = {
        id: data.id,
        name: data.name,
        email: data.email,
        picture: data.picture
      };

      this.saveUser(user);
      this.errorSignal.set(null);
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to fetch user info';
      this.errorSignal.set(errorMessage);
      console.error('User info error:', error);
    } finally {
      this.isLoadingSignal.set(false);
    }
  }

  /**
   * Sign out - Revoke access token
   */
  async signOut(): Promise<void> {
    if (this.accessToken && typeof google !== 'undefined') {
      google.accounts.oauth2.revoke(this.accessToken, () => {
        console.log('Token revoked');
      });
    }

    this.handleSignOut();
  }

  /**
   * Get current access token
   */
  getAccessToken(): string | null {
    return this.accessToken;
  }

  /**
   * Handle sign out
   */
  private handleSignOut(): void {
    this.userSignal.set(null);
    this.accessToken = null;
    this.clearLocalStorage();
  }
}
