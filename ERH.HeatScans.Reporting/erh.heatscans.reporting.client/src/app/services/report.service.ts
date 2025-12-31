import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { GoogleDriveItem } from '../models/google-drive.model';
import { Report } from '../models/report.model';
import { environment } from '../../environments/environment';

export interface ImageIndexUpdate {
  id: string;
  index: number;
}

@Injectable({
  providedIn: 'root'
})
export class ReportService {
  private baseUrl = `${environment.apiBaseUrl}report`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}
  
  getReport(folderId: string): Observable<Report> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/report?folderId=${folderId}`;
    
    return this.http.get<Report>(url, { headers });
  }

  updateImageIndices(folderId: string, indexUpdates: ImageIndexUpdate[]): Observable<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/update-indices?folderId=${folderId}`;
    
    return this.http.post<void>(url, indexUpdates, { headers });
  }
  
  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    console.log("token", token);
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
