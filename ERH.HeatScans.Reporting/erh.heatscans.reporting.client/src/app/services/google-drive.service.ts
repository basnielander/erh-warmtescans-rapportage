import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GoogleDriveItem } from '../models/google-drive.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class GoogleDriveService {
  private baseUrl = 'https://localhost:7209/api/user/googledrive';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getFolderStructure(folderId?: string): Observable<GoogleDriveItem> {
    const headers = this.getAuthHeaders();
    const url = folderId 
      ? `${this.baseUrl}/structure?folderId=${folderId}`
      : `${this.baseUrl}/structure`;
    
    return this.http.get<GoogleDriveItem>(url, { headers });
  }

  getFiles(folderId?: string): Observable<GoogleDriveItem[]> {
    const headers = this.getAuthHeaders();
    const url = folderId 
      ? `${this.baseUrl}/files?folderId=${folderId}`
      : `${this.baseUrl}/files`;
    
    return this.http.get<GoogleDriveItem[]>(url, { headers });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    console.log("token", token);
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
