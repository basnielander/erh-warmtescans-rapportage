import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MapsService {
  private baseUrl = 'https://test.nielander.nl/api/maps';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getStaticMapImage(address: string, zoom: number = 16, size: string = '600x400'): Observable<Blob> {
    const headers = this.getAuthHeaders();
    const url = `${this.baseUrl}/image?address=${encodeURIComponent(address)}&zoom=${zoom}&size=${size}`;
    
    return this.http.get(url, { 
      headers,
      responseType: 'blob'
    });
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getAccessToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
