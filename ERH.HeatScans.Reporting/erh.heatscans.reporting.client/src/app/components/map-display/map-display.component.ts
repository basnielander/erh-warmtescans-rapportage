import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MapsService } from '../../services/maps.service';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-map-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-display.component.html',
  styleUrls: ['./map-display.component.css']
})
export class MapDisplayComponent implements OnInit {
  @Input() address: string = 'Pruimengaarde 8, Houten, Netherlands';
  @Input() zoom: number = 20;
  @Input() size: string = '600x400';

  imageUrl: SafeUrl | null = null;
  isLoading: boolean = false;
  error: string | null = null;

  constructor(
    private mapsService: MapsService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.loadMap();
  }

  loadMap(): void {
    this.isLoading = true;
    this.error = null;
    this.imageUrl = null;

    this.mapsService.getStaticMapImage(this.address, this.zoom, this.size)
      .subscribe({
        next: (blob: Blob) => {
          const objectUrl = URL.createObjectURL(blob);
          this.imageUrl = this.sanitizer.bypassSecurityTrustUrl(objectUrl);
          this.isLoading = false;
        },
        error: (err) => {
          console.error('Error loading map:', err);
          this.error = err.error?.message || 'Failed to load map image';
          this.isLoading = false;
        }
      });
  }

  reload(): void {
    this.loadMap();
  }
}
