import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  getFolderStructure(): Observable<GoogleDriveItem> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/users`;
    
    return this.http.get<GoogleDriveItem>(url, { headers });
  }

  getFiles(folderId?: string): Observable<GoogleDriveItem[]> {
    const headers = this.getAuthHeaders();
    const url = folderId 
      ? `${this.baseUrl}/files?folderId=${folderId}`
      : `${this.baseUrl}/files`;
    
    return this.http.get<GoogleDriveItem[]>(url, { headers });
  }

  setupAddressFolder(addressFolderId: string): Observable<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}?addressFolderId=${addressFolderId}`;
    
    return this.http.post<void>(url, null, { headers });
  }

  getReport(folderId: string): Observable<Report> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/report?folderId=${folderId}`;
    
    return this.http.get<Report>(url, { headers });
  }
  
  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    console.log("token", token);
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
