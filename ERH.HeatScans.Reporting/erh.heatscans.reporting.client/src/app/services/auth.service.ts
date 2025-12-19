import { BehaviorSubject, Observable } from 'rxjs';
import { GoogleUser, GoogleAuthResponse, DecodedToken } from '../models/google-auth.model';
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
  private accessToken = signal<string | null>(null);





  //private currentUserSubject: BehaviorSubject<GoogleUser | null>;
  //public currentUser: Observable<GoogleUser | null>;
  //private accessTokenSubject: BehaviorSubject<string | null>;
  

  constructor() {
    // Effect to log authentication state changes
    effect(() => {
      const user = this.userSignal();
      console.log('Auth state changed:', user ? 'Logged in' : 'Logged out');
    });
    //const storedUser = localStorage.getItem('currentUser');
    //const storedToken = localStorage.getItem('accessToken');
    
    //this.currentUserSubject = new BehaviorSubject<GoogleUser | null>(
    //  storedUser ? JSON.parse(storedUser) : null
    //);
    //this.accessTokenSubject = new BehaviorSubject<string | null>(storedToken);
    
    //this.currentUser = this.currentUserSubject.asObservable();
    //this.accessToken = this.accessTokenSubject.asObservable();
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
    // apiKey: environment.googleApiKey,
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
        this.accessToken = response.access_token;
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

      this.userSignal.set(user);
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
    return this.accessToken();
  }

  /**
   * Handle sign out
   */
  private handleSignOut(): void {
    this.userSignal.set(null);
    this.accessToken.set(null);
  }

  //public get currentUserValue(): GoogleUser | null {
  //  return this.currentUserSubject.value;
  //}

  //public get accessTokenValue(): string | null {
  //  return this.accessTokenSubject.value;
  //}

  //initializeGoogleAuth(clientId: string): Promise<void> {
  //  return new Promise((resolve, reject) => {
  //    if (google == null) {
  //      reject('Google Identity Services not loaded');
  //      return;
  //    }

  //    try {
  //      google.accounts.id.initialize({
  //        client_id: clientId,
  //        callback: (response: GoogleAuthResponse) => this.handleCredentialResponse(response),
  //        auto_select: false,
  //        cancel_on_tap_outside: true,
  //      });

  //      // Initialize the token client once during auth setup
  //      this.tokenClient = google.accounts.oauth2.initTokenClient({
  //        client_id: clientId,
  //        scope: 'https://www.googleapis.com/auth/drive',
  //        callback: (tokenResponse: any) => {
  //          console.log('Access Token Response:', tokenResponse);
  //          if (tokenResponse && tokenResponse.access_token) {
  //            localStorage.setItem('accessToken', tokenResponse.access_token);
  //            this.accessTokenSubject.next(tokenResponse.access_token);
  //          } else if (tokenResponse.error) {
  //            console.error('Error getting access token:', tokenResponse.error);
  //          }
  //        },
  //      });

  //      resolve();
  //    } catch (error) {
  //      reject(error);
  //    }
  //  });
  //}

  //renderButton(element: HTMLElement): void {
  //  google.accounts.id.renderButton(
  //    element,
  //    { 
  //      theme: 'outline', 
  //      size: 'large',
  //      text: 'signin_with',
  //      shape: 'rectangular',
  //      logo_alignment: 'left'
  //    }
  //  );
  //}

  //private handleCredentialResponse(response: GoogleAuthResponse): void {
  //  console.log("handleCredentialResponse")
  //  const token = response.credential;
  //  const decoded = this.parseJwt(token);
    
  //  const user: GoogleUser = {
  //    email: decoded.email,
  //    name: decoded.name,
  //    picture: decoded.picture,
  //    sub: decoded.sub
  //  };

  //  localStorage.setItem('currentUser', JSON.stringify(user));
  //  localStorage.setItem('idToken', token);
    
  //  this.currentUserSubject.next(user);
    
  //  // Check if we have a valid access token, if not, it needs to be requested with user interaction
  //  const storedAccessToken = localStorage.getItem('accessToken');
  //  if (storedAccessToken) {
  //    this.accessTokenSubject.next(storedAccessToken);
  //  }
  //  // Note: Don't automatically request access token here - it requires user gesture
  //  // Call requestAccessToken() from a button click or user action instead
  //}

  //public requestAccessToken(): void {
  //  if (!this.tokenClient) {
  //    console.error('Token client not initialized. Call initializeGoogleAuth first.');
  //    return;
  //  }

  //  // Check if we already have a valid token
  //  const existingToken = localStorage.getItem('accessToken');
  //  if (existingToken) {
  //    this.accessTokenSubject.next(existingToken);
  //    console.log('Using existing access token');
  //    return;
  //  }

  //  console.log('Requesting new access token - requires user interaction');
  //  // This MUST be called from a user gesture (button click, etc.)
  //  try {
  //    this.tokenClient.requestAccessToken({ prompt: '' });
  //  } catch (error) {
  //    console.error('Error requesting access token:', error);
  //  }
  //}

  //private parseJwt(token: string): DecodedToken {
  //  const base64Url = token.split('.')[1];
  //  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
  //  const jsonPayload = decodeURIComponent(
  //    atob(base64)
  //      .split('')
  //      .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
  //      .join('')
  //  );
  //  return JSON.parse(jsonPayload);
  //}

  //logout(): void {
  //  localStorage.removeItem('currentUser');
  //  localStorage.removeItem('idToken');
  //  localStorage.removeItem('accessToken');
  //  this.currentUserSubject.next(null);
  //  this.accessTokenSubject.next(null);
  //  google.accounts.id.disableAutoSelect();
    
  //  // Revoke the access token if it exists
  //  if (this.tokenClient) {
  //    google.accounts.oauth2.revoke(this.accessTokenValue || '', () => {
  //      console.log('Access token revoked');
  //    });
  //  }
  //}

  //isAuthenticated(): boolean {
  //  return this.currentUserValue !== null && this.accessTokenValue !== null;
  //}
}
