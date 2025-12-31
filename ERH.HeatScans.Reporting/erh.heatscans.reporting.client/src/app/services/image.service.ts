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
export class ImageService {
  private baseUrl = `${environment.apiBaseUrl}image`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}
  
  getImage(fileId: string): Observable<Blob> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}?fileId=${fileId}`;
    
    return this.http.get(url, { 
      headers, 
      responseType: 'blob' 
    });
  }

  addSpot(imageFileId: string, x: number, y: number): Observable<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/spot?imageFileId=${imageFileId}&x=${x}&y=${y}`;
    
    return this.http.post<void>(url, null, { headers });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    console.log("token", token);
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
