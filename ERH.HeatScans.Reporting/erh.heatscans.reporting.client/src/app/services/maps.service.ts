import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MapsService {
  private baseUrl = `${environment.apiBaseUrl}maps`;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  async getStaticMapImage(address: string, zoom: number = 16, size: string = '600x400'): Promise<Blob> {
    const headers = await this.authService.getAuthHeaders();
    const url = `${this.baseUrl}/image?address=${encodeURIComponent(address)}&zoom=${zoom}&size=${size}`;
    
    return lastValueFrom(this.http.get(url, { 
      headers,
      responseType: 'blob'
    }));
  }
}
