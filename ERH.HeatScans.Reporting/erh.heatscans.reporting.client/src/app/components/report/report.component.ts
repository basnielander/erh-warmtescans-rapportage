import { Component, OnInit, signal, effect, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { MapDisplayComponent } from '../map-display/map-display.component';
import { ImageCardComponent } from '../image-card/image-card.component';
import { BatchOutdoorCalibrationComponent } from '../batch-outdoor-calibration/batch-outdoor-calibration.component';
import { BatchIndoorCalibrationComponent } from '../batch-indoor-calibration/batch-indoor-calibration.component';
import { ModalComponent } from '../modal/modal.component';
import { ReportDetailsEditorComponent } from '../report-details-editor/report-details-editor.component';
import { FoldersAndFileService } from '../../services/folders-and-files.service';
import { Report } from '../../models/report.model';
import { ImageInfo } from "../../models/image-info.model";
import { toSignal } from '@angular/core/rxjs-interop';
import { ReportService } from '../../services/report.service';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, MapDisplayComponent, ImageCardComponent, BatchOutdoorCalibrationComponent, BatchIndoorCalibrationComponent, ModalComponent, ReportDetailsEditorComponent],
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.scss']
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

  // Batch calibration state
  showBatchOutdoorCalibration = signal<boolean>(false);
  showBatchIndoorCalibration = signal<boolean>(false);
  showReportDetailsEditor = signal<boolean>(false);
  isExportingReport = signal<boolean>(false);

  // Computed signal for sorted images
  sortedImages = computed(() => {
    const report = this.addressReport();
    if (!report || !report.images) return [];
    return [...report.images].sort((a, b) => a.index - b.index);
  });

  // Computed signal for excluded images
  excludedImages = computed(() => {
    return this.sortedImages().filter((image) => image.excludeFromReport);
  });

  // Computed signal for all non-excluded images
  allIncludedImages = computed(() => {
    return this.sortedImages().filter((image) => !image.excludeFromReport);
  });

  indoorImagesAvailable = computed(() => {
    if (this.sortedImages().length === 0) {
      return false;
    }
    return this.sortedImages().some((image) => !image.outdoor);
  });

  outdoorImagesAvailable = computed(() => {
    if (this.sortedImages().length === 0) {
      return false;
    }
    return this.sortedImages().some((image) => image.outdoor);
  });

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private driveService: FoldersAndFileService,
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

  async setupAddressFolder(): Promise<void> {
    this.isSettingUp.set(true);
    this.setupError.set(null);

    try {
      await this.driveService.setupAddressFolder(this.folderId());
      console.log('Address folder setup completed successfully');
      this.isSettingUp.set(false);
      // Call getReport after successful setup
      await this.loadReport();
    } catch (err: any) {
      console.error('Error setting up address folder:', err);
      this.setupError.set('Failed to setup address folder. Please try again.');
      this.isSettingUp.set(false);
    }
  }

  async loadReport(): Promise<void> {
    this.isLoadingReport.set(true);
    this.reportError.set(null);

    try {
      const report = await this.reportService.getReport(this.folderId());
      console.log('Report loaded successfully:', report);
      this.addressReport.set(report);
    } catch (err: any) {
      console.error('Error loading report:', err);
      this.reportError.set('Failed to load report. Please try again.');
    } finally {
      this.isLoadingReport.set(false);
    }
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

  async updateImageIndices(images: ImageInfo[]): Promise<void> {
    this.isUpdatingIndices.set(true);
    
    const indexUpdates = images.map(img => ({
      id: img.id,
      index: img.index
    }));

    try {
      await this.reportService.updateImageIndices(this.folderId(), indexUpdates);
      console.log('Image indices updated successfully');
    } catch (err: any) {
      console.error('Error updating image indices:', err);
      // Optionally reload the report to restore the correct order
      await this.loadReport();
    } finally {
      this.isUpdatingIndices.set(false);
    }
  }
  
  async onToggleExcludeImage(imageId: string): Promise<void> {
    const currentReport = this.addressReport();
    if (!currentReport) return;

    const image = currentReport.images.find(img => img.id === imageId);
    if (!image) return;

    const shouldExclude = !image.excludeFromReport;

    try {
      await this.reportService.toggleImageExclusion(this.folderId(), imageId, shouldExclude);
      console.log(`Image ${shouldExclude ? 'excluded from' : 'included in'} report successfully`);
      // Update the local state
      const updatedImages = currentReport.images.map(img => 
        img.id === imageId ? { ...img, excludeFromReport: shouldExclude } : img
      );
      this.addressReport.set({
        ...currentReport,
        images: updatedImages
      });
    } catch (err: any) {
      console.error('Error toggling image exclusion:', err);
    }
  }

  async onUpdateImageProperties(data: { imageId: string, comment: string, outdoor: boolean }): Promise<void> {
    const currentReport = this.addressReport();
    if (!currentReport) return;

    try {
      await this.reportService.updateImageProperties(this.folderId(), data.imageId, data.comment, data.outdoor);
      console.log('Image properties updated successfully');
      // Update the local state
      const updatedImages = currentReport.images.map(img => 
        img.id === data.imageId ? { ...img, comment: data.comment, outdoor: data.outdoor } : img
      );
      this.addressReport.set({
        ...currentReport,
        images: updatedImages
      });
    } catch (err: any) {
      console.error('Error updating image properties:', err);
    }
  }

  async onUpdateCalibration(data: { imageId: string, temperatureMin: number, temperatureMax: number }): Promise<void> {
    const currentReport = this.addressReport();
    if (!currentReport) return;

    try {
      await this.reportService.updateImageCalibration(this.folderId(), data.imageId, data.temperatureMin, data.temperatureMax);
      console.log('Image calibration updated successfully');
      // Update the local state
      const updatedImages = currentReport.images.map(img => 
        img.id === data.imageId ? { ...img, temperatureMin: data.temperatureMin, temperatureMax: data.temperatureMax } : img
      );
      this.addressReport.set({
        ...currentReport,
        images: updatedImages
      });
    } catch (err: any) {
      console.error('Error updating image calibration:', err);
    }
  }

  onShowBatchOutdoorCalibration(): void {
    this.showBatchOutdoorCalibration.set(true);
  }

  onShowBatchIndoorCalibration(): void {
    this.showBatchIndoorCalibration.set(true);
  }

  async onBatchCalibrationSave(data: { imageIds: string[], temperatureMin: number, temperatureMax: number }): Promise<void> {
    try {
      await this.reportService.batchCalibrateImages(data.imageIds, data.temperatureMin, data.temperatureMax);
      console.log('Batch calibration completed successfully');
      // Update local state for all images
      const currentReport = this.addressReport();
      if (currentReport) {
        const updatedImages = currentReport.images.map(img => 
          data.imageIds.includes(img.id) 
            ? { ...img, temperatureMin: data.temperatureMin, temperatureMax: data.temperatureMax } 
            : img
        );
        this.addressReport.set({
          ...currentReport,
          images: updatedImages
        });
      }
      this.showBatchOutdoorCalibration.set(false);
      this.showBatchIndoorCalibration.set(false);
    } catch (err: any) {
      console.error('Error in batch calibration:', err);
      alert('Batch calibration failed. Please try again.');
      this.showBatchOutdoorCalibration.set(false);
      this.showBatchIndoorCalibration.set(false);
    }
  }

  onBatchCalibrationCancel(): void {
    this.showBatchOutdoorCalibration.set(false);
    this.showBatchIndoorCalibration.set(false);
  }

  onShowReportDetailsEditor(): void {
    this.showReportDetailsEditor.set(true);
  }

  async onReportDetailsSave(updatedDetails: Partial<Report>): Promise<void> {
    const currentReport = this.addressReport();
    if (!currentReport) return;

    try {
      await this.reportService.updateReportDetails(this.folderId(), updatedDetails);
      console.log('Report details updated successfully');
      // Update local state
      this.addressReport.set({
        ...currentReport,
        ...updatedDetails
      });
      this.showReportDetailsEditor.set(false);
    } catch (err: any) {
      console.error('Error updating report details:', err);
      alert('Failed to update report details. Please try again.');
    }
  }

  onReportDetailsCancel(): void {
    this.showReportDetailsEditor.set(false);
  }

  async onExportReport(): Promise<void> {
    this.isExportingReport.set(true);

    try {
      const blob = await this.reportService.createReport(this.folderId());
      console.log('Report exported successfully:');
     
      
      // Create a download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `report-${this.folderName()}-${new Date().toISOString().split('T')[0]}.docx`;
      
      // Trigger download
      document.body.appendChild(link);
      link.click();
      
      // Cleanup
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
      console.log('Report file downloaded');
    } catch (err: any) {
      console.error('Error exporting report:', err);
      alert('Failed to export report. Please try again.');
    } finally {
      this.isExportingReport.set(false);
    }
  }
}
