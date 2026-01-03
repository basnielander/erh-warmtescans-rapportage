import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ImageScale } from '../../models/image-scale.model';

@Component({
  selector: 'app-image-scale',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-scale.component.html',
  styleUrl: './image-scale.component.scss'
})
export class ImageScaleComponent {
  // Input signal for the image scale data - now optional
  imageScale = input<ImageScale | null | undefined>();

  constructor(private sanitizer: DomSanitizer) {}

  /**
   * Get the safe URL for the image
   */
  getImageUrl(): SafeUrl | null {
    const imageScale = this.imageScale();
    if (!imageScale || !imageScale.data || !imageScale.mimeType) {
      return null;
    }
    const dataUrl = `data:${imageScale.mimeType};base64,${imageScale.data}`;
    return this.sanitizer.bypassSecurityTrustUrl(dataUrl);
  }

  /**
   * Format temperature with 1 decimal place and °C
   */
  formatTemperature(temp: number | undefined): string {
    if (temp === undefined || temp === null) {
      return 'N/A';
    }
    return `${temp.toFixed(1)}°C`;
  }

  /**
   * Check if the component has valid scale data
   */
  hasValidScale(): boolean {
    const scale = this.imageScale();
    return !!(scale && scale.data && scale.mimeType);
  }
}
