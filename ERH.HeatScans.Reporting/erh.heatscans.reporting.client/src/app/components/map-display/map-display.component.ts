import { Component, OnInit, input, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MapsService } from '../../services/maps.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-map-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-display.component.html',
  styleUrls: ['./map-display.component.scss']
})
export class MapDisplayComponent implements OnInit {
  // Input signals
  address = input<string>('Pruimengaarde 8');
  zoom = input<number>(20);
  size = input<string>('600x400');

  // State signals
  imageUrl = signal<SafeUrl | null>(null);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);

  constructor(
    private mapsService: MapsService,
    private sanitizer: DomSanitizer
  ) {
    // Effect to reload map when inputs change
    effect(() => {
      // Access the signals to track them
      const currentAddress = this.address();
      const currentZoom = this.zoom();
      const currentSize = this.size();
      
      // Load map when any input changes
      if (currentAddress) {
        this.loadMap();
      }
    });
  }

  ngOnInit(): void {
    // Initial load is handled by the effect
  }

  async loadMap(): Promise<void> {
    this.isLoading.set(true);
    this.error.set(null);
    this.imageUrl.set(null);

    try {
      const blob = await this.mapsService.getStaticMapImage(this.address(), this.zoom(), this.size());
      const objectUrl = URL.createObjectURL(blob);
      this.imageUrl.set(this.sanitizer.bypassSecurityTrustUrl(objectUrl));
    } catch (err: any) {
      console.error('Error loading map:', err);
      this.error.set(err.error?.message || 'Failed to load map image');
    } finally {
      this.isLoading.set(false);
    }
  }

  reload(): void {
    this.loadMap();
  }
}
