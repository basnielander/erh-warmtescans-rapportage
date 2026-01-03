import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { GoogleDriveItem } from '../models/google-drive.model';
import { Report } from '../models/report.model';
import { Image } from "../models/image.model";
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ImageService {
  private baseUrl = `${environment.apiBaseUrl}images`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}
  
  getImage(fileId: string): Promise<Image> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/${fileId}`;
    
    return lastValueFrom(this.http.get<Image>(url, { headers }));
  }

  addSpot(imageFileId: string, x: number, y: number): Promise<Image> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/${imageFileId}/spots?relativeX=${x}&relativeY=${y}`;
    
    return lastValueFrom(this.http.post<Image>(url, null, { headers }));
  }

  deleteSpot(imageFileId: string, spotName: string): Promise<Image> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/${imageFileId}/spots/${spotName}`;
    
    return lastValueFrom(this.http.delete<Image>(url, { headers }));
  }

  /**
   * Helper method to convert Image data to Blob for displaying in img tags
   */
  imageToBlob(image: Image): Blob {
    // Decode base64 string to binary data
    const binaryString = atob(image.data);
    const len = binaryString.length;
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = binaryString.charCodeAt(i);
    }
    return new Blob([bytes], { type: image.mimeType });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();

    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
