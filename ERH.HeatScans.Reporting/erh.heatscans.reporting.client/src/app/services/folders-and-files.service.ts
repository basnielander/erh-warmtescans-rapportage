import { Injectable, signal, computed } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { GoogleDriveItem } from '../models/google-drive.model';
import { Report } from '../models/report.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FoldersAndFileService {
  private baseUrl = `${environment.apiBaseUrl}folders-and-files`;

  // Signals for state management
  private folderStructureSignal = signal<GoogleDriveItem | null>(null);
  private isLoadingSignal = signal<boolean>(false);
  private errorSignal = signal<string | null>(null);

  // Public readonly signals
  folderStructure = this.folderStructureSignal.asReadonly();
  isLoading = this.isLoadingSignal.asReadonly();
  error = this.errorSignal.asReadonly();
  hasFolderStructure = computed(() => this.folderStructureSignal() !== null);

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  async getFolderStructure(): Promise<GoogleDriveItem> {
    this.isLoadingSignal.set(true);
    this.errorSignal.set(null);

    try {
      const headers = this.getAuthHeaders();
      const url = `${this.baseUrl}/users`;
      
      const data = await lastValueFrom(this.http.get<GoogleDriveItem>(url, { headers }));
      
      // Update state with the retrieved folder structure
      this.folderStructureSignal.set(data);
      
      return data;
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Failed to load folder structure';
      this.errorSignal.set(errorMessage);
      throw error;
    } finally {
      this.isLoadingSignal.set(false);
    }
  }
  
  setupAddressFolder(addressFolderId: string): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}?addressFolderId=${addressFolderId}`;
    
    return lastValueFrom(this.http.post<void>(url, null, { headers }));
  }

  /**
   * Clear the folder structure state
   */
  clearFolderStructure(): void {
    this.folderStructureSignal.set(null);
    this.errorSignal.set(null);
  }
  
  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();

    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
