import { Component, Input, Output, EventEmitter, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ImageInfo } from '../../models/report.model';
import { GoogleDriveService } from '../../services/folders-and-files.service';
import { ImageService } from '../../services/image.service';

@Component({
  selector: 'app-image-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-card.component.html',
  styleUrl: './image-card.component.css'
})
export class ImageCardComponent implements OnInit {
  @Input() image!: ImageInfo;
  @Output() toggleExclude = new EventEmitter<string>();
  
  imageUrl = signal<SafeUrl | null>(null);
  isLoading = signal<boolean>(true);
  hasError = signal<boolean>(false);

  constructor(
    private driveService: GoogleDriveService,
    private imageService: ImageService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.loadImage();
  }

  loadImage(): void {
    this.isLoading.set(true);
    this.hasError.set(false);

    this.imageService.getImage(this.image.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const safeUrl = this.sanitizer.bypassSecurityTrustUrl(url);
        this.imageUrl.set(safeUrl);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(`Error loading image ${this.image.name}:`, err);
        this.hasError.set(true);
        this.isLoading.set(false);
      }
    });
  }

  onImageClick(event: MouseEvent): void {
    const imgElement = event.target as HTMLImageElement;
    const rect = imgElement.getBoundingClientRect();
    
    const x = Math.round(event.clientX - rect.left);
    const y = Math.round(event.clientY - rect.top);

    console.log(`Adding spot at coordinates: x=${x}, y=${y} for image ${this.image.id}`);

    this.imageService.addSpot(this.image.id, x, y).subscribe({
      next: () => {
        console.log('Spot added successfully');
      },
      error: (err) => {
        console.error('Error adding spot:', err);
      }
    });
  }

  onToggleExcludeClick(event: MouseEvent): void {
    event.stopPropagation();
    this.toggleExclude.emit(this.image.id);
  }

  formatFileSize(bytes: number | undefined): string {
    if (!bytes) return '';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    return (bytes / (1024 * 1024 * 1024)).toFixed(1) + ' GB';
  }
}
