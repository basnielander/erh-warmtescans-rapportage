import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { GoogleDriveItem } from '../models/google-drive.model';
import { Report } from '../models/report.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class GoogleDriveService {
  private baseUrl = `${environment.apiBaseUrl}folders-and-files`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getFolderStructure(): Promise<GoogleDriveItem> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/users`;
    
    return lastValueFrom(this.http.get<GoogleDriveItem>(url, { headers }));
  }

  getFiles(folderId?: string): Promise<GoogleDriveItem[]> {
    const headers = this.getAuthHeaders();
    const url = folderId 
      ? `${this.baseUrl}/files?folderId=${folderId}`
      : `${this.baseUrl}/files`;
    
    return lastValueFrom(this.http.get<GoogleDriveItem[]>(url, { headers }));
  }

  setupAddressFolder(addressFolderId: string): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}?addressFolderId=${addressFolderId}`;
    
    return lastValueFrom(this.http.post<void>(url, null, { headers }));
  }
  
  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();

    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
