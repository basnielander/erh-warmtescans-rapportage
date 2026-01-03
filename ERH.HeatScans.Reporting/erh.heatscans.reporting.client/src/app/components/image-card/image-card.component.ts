import { Component, output, OnInit, signal, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ImageInfo } from "../../models/image-info.model";
import { Image } from "../../models/image.model";
import { FoldersAndFileService } from '../../services/folders-and-files.service';
import { ImageService } from '../../services/image.service';
import { ImageScaleComponent } from '../image-scale/image-scale.component';

@Component({
  selector: 'app-image-card',
  standalone: true,
  imports: [CommonModule, FormsModule, ImageScaleComponent],
  templateUrl: './image-card.component.html',
  styleUrl: './image-card.component.css'
})
export class ImageCardComponent implements OnInit {
  // Input signal instead of @Input decorator
  image = input.required<ImageInfo>();
  
  toggleExclude = output<string>();
  updateImageProperties = output<{ imageId: string, comment: string, outdoor: boolean }>();
  
  imageUrl = signal<SafeUrl | null>(null);
  isLoading = signal<boolean>(true);
  hasError = signal<boolean>(false);
  isEditing = signal<boolean>(false);
  
  // Local state for editing
  editComment = signal<string>('');
  editOutdoor = signal<boolean>(true);

  // Store image data for displaying spots
  currentImage = signal<Image | null>(null);

  constructor(
    private driveService: FoldersAndFileService,
    private imageService: ImageService,
    private sanitizer: DomSanitizer
  ) {
    // Use effect to handle image changes
    effect(() => {
      const currentImage = this.image();
      this.editComment.set(currentImage.comments || '');
      this.editOutdoor.set(currentImage.outdoor ?? true);
      this.loadImage();
    });
  }

  ngOnInit(): void {
    // Initialization now handled in constructor effect
  }

  async loadImage(): Promise<void> {
    this.isLoading.set(true);
    this.hasError.set(false);

    try {
      const imageData = await this.imageService.getImage(this.image().id);
      this.currentImage.set(imageData);
      
      // Convert Image to Blob using helper method
      const blob = this.imageService.imageToBlob(imageData);
      const url = URL.createObjectURL(blob);
      const safeUrl = this.sanitizer.bypassSecurityTrustUrl(url);
      this.imageUrl.set(safeUrl);
    } catch (err: any) {
      console.error(`Error loading image ${this.image().name}:`, err);
      this.hasError.set(true);
    } finally {
      this.isLoading.set(false);
    }
  }

  async onImageClick(event: MouseEvent): Promise<void> {
    const imgElement = event.target as HTMLImageElement;
    const rect = imgElement.getBoundingClientRect();
    
    const x = (event.clientX - rect.left) / rect.width;
    const y = (event.clientY - rect.top) / rect.height;

    try {
      const imageData = await this.imageService.addSpot(this.image().id, x, y);
      this.currentImage.set(imageData);
      
      // Update the displayed image with the new version using helper method
      const blob = this.imageService.imageToBlob(imageData);
      const url = URL.createObjectURL(blob);
      const safeUrl = this.sanitizer.bypassSecurityTrustUrl(url);
      this.imageUrl.set(safeUrl);
    } catch (err: any) {
      console.error('Error adding spot:', err);
    }
  }

  onToggleExcludeClick(event: MouseEvent): void {
    event.stopPropagation();
    this.toggleExclude.emit(this.image().id);
  }

  onEditClick(event: MouseEvent): void {
    event.stopPropagation();
    this.isEditing.set(true);
    this.editComment.set(this.image().comments || '');
    this.editOutdoor.set(this.image().outdoor ?? true);
  }

  onSaveClick(event: MouseEvent): void {
    event.stopPropagation();
    this.updateImageProperties.emit({
      imageId: this.image().id,
      comment: this.editComment(),
      outdoor: this.editOutdoor()
    });
    this.isEditing.set(false);
  }

  onCancelClick(event: MouseEvent): void {
    event.stopPropagation();
    this.isEditing.set(false);
    this.editComment.set(this.image().comments || '');
    this.editOutdoor.set(this.image().outdoor ?? true);
  }

  async onDeleteSpotClick(event: MouseEvent, spotName: string): Promise<void> {
    event.stopPropagation();
    
    console.log(`Deleting spot ${spotName} from image ${this.image().id}`);

    try {
      const imageData = await this.imageService.deleteSpot(this.image().id, spotName);
      this.currentImage.set(imageData);
      console.log('Spot deleted successfully', imageData.spots);
      
      // Update the displayed image with the new version using helper method
      const blob = this.imageService.imageToBlob(imageData);
      const url = URL.createObjectURL(blob);
      const safeUrl = this.sanitizer.bypassSecurityTrustUrl(url);
      this.imageUrl.set(safeUrl);
    } catch (err: any) {
      console.error('Error deleting spot:', err);
    }
  }

  formatFileSize(bytes: number | undefined): string {
    if (!bytes) return '';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    return (bytes / (1024 * 1024 * 1024)).toFixed(1) + ' GB';
  }
}
