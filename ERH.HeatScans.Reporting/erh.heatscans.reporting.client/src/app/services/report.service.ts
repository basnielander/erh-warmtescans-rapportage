import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
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
  
  getReport(folderId: string): Promise<Report> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}?folderId=${folderId}`;
    
    return lastValueFrom(this.http.get<Report>(url, { headers }));
  }

  createReportDocument(folderId: string): Promise<Blob> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/document?folderId=${folderId}`;

    return lastValueFrom(this.http.post<Blob>(url, {}, { headers }));
  }

  updateImageIndices(folderId: string, indexUpdates: ImageIndexUpdate[]): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/update-indices?folderId=${folderId}`;
    
    return lastValueFrom(this.http.post<void>(url, indexUpdates, { headers }));
  }
  
  toggleImageExclusion(folderId: string, imageId: string, exclude: boolean): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/toggle-image-exclusion?imageId=${imageId}&exclude=${exclude}`;
    
    return lastValueFrom(this.http.post<void>(url, null, { headers }));
  }

  updateImageProperties(folderId: string, imageId: string, comment: string, outdoor: boolean): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/update-image-properties?imageId=${imageId}&comment=${encodeURIComponent(comment)}&outdoor=${outdoor}`;
    
    return lastValueFrom(this.http.patch<void>(url, null, { headers }));
  }

  updateImageCalibration(folderId: string, imageId: string, temperatureMin: number, temperatureMax: number): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/update-image-calibration?imageId=${imageId}&temperatureMin=${temperatureMin}&temperatureMax=${temperatureMax}`;
    
    return lastValueFrom(this.http.post<void>(url, null, { headers }));
  }

  batchCalibrateImages(imageIds: string[], temperatureMin: number, temperatureMax: number): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${environment.apiBaseUrl}images/calibrate`;
    
    const calibrationRequest = {
      imageFileIds: imageIds,
      minTemperature: temperatureMin,
      maxTemperature: temperatureMax
    };
    
    return lastValueFrom(this.http.post<void>(url, calibrationRequest, { headers }));
  }

  updateReportDetails(folderId: string, details: Partial<Report>): Promise<void> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/update-report-details?folderId=${folderId}`;
    
    return lastValueFrom(this.http.patch<void>(url, details, { headers }));
  }
  
  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();

    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
