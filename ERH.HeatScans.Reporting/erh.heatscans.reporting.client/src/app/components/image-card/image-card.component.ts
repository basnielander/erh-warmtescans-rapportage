import { Component, output, OnInit, signal, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { ImageInfo } from "../../models/image-info.model";
import { Image } from "../../models/image.model";
import { FoldersAndFileService } from '../../services/folders-and-files.service';
import { ImageService } from '../../services/image.service';
import { ImageScaleComponent } from '../image-scale/image-scale.component';
import { ImageEditFormComponent } from '../image-edit-form/image-edit-form.component';
import { TemperatureSpotsListComponent } from '../temperature-spots-list/temperature-spots-list.component';

@Component({
  selector: 'app-image-card',
  standalone: true,
  imports: [CommonModule, FormsModule, ImageScaleComponent, ImageEditFormComponent, TemperatureSpotsListComponent],
  templateUrl: './image-card.component.html',
  styleUrl: './image-card.component.scss'
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

  // Store image data for displaying spots
  currentImage = signal<Image | null>(null);

  // Track daylight photo collapsed state
  isDaylightPhotoCollapsed = signal<boolean>(true);

  constructor(
    private driveService: FoldersAndFileService,
    private imageService: ImageService,
    private sanitizer: DomSanitizer
  ) {
    // Use effect to handle image changes
    effect(() => {
      const currentImage = this.image();
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
    const img = this.currentImage();

    if (img == null) {
      return;
    }

    const imgElement = event.target as HTMLImageElement;
    const rect = imgElement.getBoundingClientRect();
    
    // Calculate relative position in the displayed image
    

    const imageResolution = (img.size.width / img.size.height);
    const imageElementResolution = (rect.width / rect.height);

    const localClickPositionX = (event.clientX - rect.left);
    const localClickPositionY = (event.clientY - rect.top)

    // Calculate coordinates based on actual image dimensions
    let x = 0;
    let y = 0;

    if (imageResolution > imageElementResolution) {

      const overflowHalfHeight = (rect.height - (rect.width / imageResolution)) / 2;

      if (localClickPositionY < overflowHalfHeight) {
        return; // clicked above the image
      }
      if (localClickPositionY > rect.height - overflowHalfHeight) {
        return; // clicked below the image
      }

      x = localClickPositionX / rect.width;
      y = (localClickPositionY - overflowHalfHeight) / (rect.height - (2 * overflowHalfHeight));

    } else if (imageResolution < imageElementResolution) {

      const overflowHalfWidth = (rect.width - (rect.height * imageResolution)) / 2;

      if (localClickPositionX < overflowHalfWidth) {
        return; // clicked left of the image
      }
      if (localClickPositionX > rect.width - overflowHalfWidth) {
        return; // clicked right of the image
      }

      x = (localClickPositionX - overflowHalfWidth) / (rect.width - (2 * overflowHalfWidth));
      y = localClickPositionY / rect.height;
    } else {
      x = localClickPositionX / rect.width;
      y = localClickPositionY / rect.height;
    }

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

  onFormSave(data: { comment: string, outdoor: boolean }): void {
    this.updateImageProperties.emit({
      imageId: this.image().id,
      comment: data.comment,
      outdoor: data.outdoor
    });
  }

  onFormCancel(): void {
    // Reset handled by the child component
  }

  async onDeleteSpotClick(spotName: string): Promise<void> {
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

  toggleDaylightPhoto(): void {
    this.isDaylightPhotoCollapsed.set(!this.isDaylightPhotoCollapsed());
  }
}
