import { Component, OnInit, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { MapDisplayComponent } from '../map-display/map-display.component';
import { ImageCardComponent } from '../image-card/image-card.component';
import { GoogleDriveService } from '../../services/folders-and-files.service';
import { Report, ImageInfo } from '../../models/report.model';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReportService } from '../../services/report.service';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, MapDisplayComponent, ImageCardComponent],
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
  // Convert route params to signals
  params = signal<ParamMap | null>(null);
  
  folderName = signal<string>('');
  folderId = signal<string>('');
  isSettingUp = signal<boolean>(false);
  setupError = signal<string | null>(null);
  
  // Signal to store the address report
  addressReport = signal<Report | null>(null);
  isLoadingReport = signal<boolean>(false);
  reportError = signal<string | null>(null);

  // Drag and drop state
  draggedIndex = signal<number | null>(null);
  isUpdatingIndices = signal<boolean>(false);

  // Computed signal for sorted images
  sortedImages = computed(() => {
    const report = this.addressReport();
    if (!report || !report.images) return [];
    return [...report.images].sort((a, b) => a.index - b.index);
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private driveService: GoogleDriveService,
    private reportService: ReportService
  ) {
    this.params.set(toSignal(this.route?.paramMap)() ?? null);

    // Use effect to react to route parameter changes
    effect(() => {
      const paramMap = this.params();
      if (paramMap) {
        const folderId = paramMap.get('folderId') || '';
        const folderName = paramMap.get('folderName') || '';
        
        this.folderId.set(folderId);
        this.folderName.set(folderName);
        
        // Call the setup API when folder ID is available
        if (folderId) {
          this.setupAddressFolder();
        }
      }
    });
  }

  ngOnInit(): void {
    // Initialization is now handled in the constructor's effect
  }

  setupAddressFolder(): void {
    this.isSettingUp.set(true);
    this.setupError.set(null);

    this.driveService.setupAddressFolder(this.folderId()).subscribe({
      next: () => {
        console.log('Address folder setup completed successfully');
        this.isSettingUp.set(false);
        // Call getReport after successful setup
        this.loadReport();
      },
      error: (err) => {
        console.error('Error setting up address folder:', err);
        this.setupError.set('Failed to setup address folder. Please try again.');
        this.isSettingUp.set(false);
      }
    });
  }

  loadReport(): void {
    this.isLoadingReport.set(true);
    this.reportError.set(null);

    this.reportService.getReport(this.folderId()).subscribe({
      next: (report) => {
        console.log('Report loaded successfully:', report);
        this.addressReport.set(report);
        this.isLoadingReport.set(false);
      },
      error: (err) => {
        console.error('Error loading report:', err);
        this.reportError.set('Failed to load report. Please try again.');
        this.isLoadingReport.set(false);
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  // Drag and drop methods
  onDragStart(event: DragEvent, index: number): void {
    this.draggedIndex.set(index);
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/html', '');
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }
  }

  onDrop(event: DragEvent, dropIndex: number): void {
    event.preventDefault();
    const dragIndex = this.draggedIndex();
    
    if (dragIndex === null || dragIndex === dropIndex) {
      this.draggedIndex.set(null);
      return;
    }

    const images = [...this.sortedImages()];
    const [draggedImage] = images.splice(dragIndex, 1);
    images.splice(dropIndex, 0, draggedImage);

    // Update indices
    const updatedImages = images.map((img, idx) => ({
      ...img,
      index: idx
    }));

    // Update the report with new order
    const currentReport = this.addressReport();
    if (currentReport) {
      this.addressReport.set({
        ...currentReport,
        images: updatedImages
      });
    }

    // Send update to server
    this.updateImageIndices(updatedImages);
    
    this.draggedIndex.set(null);
  }

  onDragEnd(): void {
    this.draggedIndex.set(null);
  }

  updateImageIndices(images: ImageInfo[]): void {
    this.isUpdatingIndices.set(true);
    
    const indexUpdates = images.map(img => ({
      id: img.id,
      index: img.index
    }));

    this.reportService.updateImageIndices(this.folderId(), indexUpdates).subscribe({
      next: () => {
        console.log('Image indices updated successfully');
        this.isUpdatingIndices.set(false);
      },
      error: (err) => {
        console.error('Error updating image indices:', err);
        this.isUpdatingIndices.set(false);
        // Optionally reload the report to restore the correct order
        this.loadReport();
      }
    });
  }
}
