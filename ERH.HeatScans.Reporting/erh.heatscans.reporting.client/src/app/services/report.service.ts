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
  
  async getReport(folderId: string): Promise<Report> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}?folderId=${folderId}`;
    
    return lastValueFrom(this.http.get<Report>(url, { headers }));
  }

  async createReportDocument(folderId: string): Promise<Blob> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}/document?folderId=${folderId}`;

    return lastValueFrom(this.http.post<Blob>(url, {}, { headers }));
  }

  async updateImageIndices(folderId: string, indexUpdates: ImageIndexUpdate[]): Promise<void> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}/update-indices?folderId=${folderId}`;
    
    return lastValueFrom(this.http.post<void>(url, indexUpdates, { headers }));
  }
  
  async toggleImageExclusion(folderId: string, imageId: string, exclude: boolean): Promise<void> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}/toggle-image-exclusion?imageId=${imageId}&exclude=${exclude}`;
    
    return lastValueFrom(this.http.post<void>(url, null, { headers }));
  }

  async updateImageProperties(folderId: string, imageId: string, comment: string, outdoor: boolean): Promise<void> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}/update-image-properties?imageId=${imageId}&comment=${encodeURIComponent(comment)}&outdoor=${outdoor}`;
    
    return lastValueFrom(this.http.patch<void>(url, null, { headers }));
  }

  async updateImageCalibration(folderId: string, imageId: string, temperatureMin: number, temperatureMax: number): Promise<void> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}/update-image-calibration?imageId=${imageId}&temperatureMin=${temperatureMin}&temperatureMax=${temperatureMax}`;
    
    return lastValueFrom(this.http.post<void>(url, null, { headers }));
  }

  async batchCalibrateImages(imageIds: string[], temperatureMin: number, temperatureMax: number): Promise<void> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${environment.apiBaseUrl}images/calibrate`;
    
    const calibrationRequest = {
      imageFileIds: imageIds,
      minTemperature: temperatureMin,
      maxTemperature: temperatureMax
    };
    
    return lastValueFrom(this.http.post<void>(url, calibrationRequest, { headers }));
  }

  async updateReportDetails(folderId: string, details: Partial<Report>): Promise<void> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}/update-report-details?folderId=${folderId}`;
    
    return lastValueFrom(this.http.patch<void>(url, details, { headers }));
  }
}
