import { Component, OnInit, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { MapDisplayComponent } from '../map-display/map-display.component';
import { GoogleDriveService } from '../../services/folders-and-files.service';
import { Report, ImageInfo } from '../../models/report.model';
import { toSignal } from '@angular/core/rxjs-interop';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, MapDisplayComponent],
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
  
  // Store image URLs
  imageUrls = new Map<string, SafeUrl>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private driveService: GoogleDriveService,
    private sanitizer: DomSanitizer
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

    this.driveService.getReport(this.folderId()).subscribe({
      next: (report) => {
        console.log('Report loaded successfully:', report);
        this.addressReport.set(report);
        this.isLoadingReport.set(false);
        // Load images after report is loaded
        this.loadImages(report.images);
      },
      error: (err) => {
        console.error('Error loading report:', err);
        this.reportError.set('Failed to load report. Please try again.');
        this.isLoadingReport.set(false);
      }
    });
  }

  loadImages(images: ImageInfo[]): void {
    images.forEach(image => {
      this.driveService.getImage(image.id).subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const safeUrl = this.sanitizer.bypassSecurityTrustUrl(url);
          this.imageUrls.set(image.id, safeUrl);
        },
        error: (err) => {
          console.error(`Error loading image ${image.name}:`, err);
        }
      });
    });
  }

  getImageUrl(fileId: string): SafeUrl | undefined {
    return this.imageUrls.get(fileId);
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    return (bytes / (1024 * 1024 * 1024)).toFixed(1) + ' GB';
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}
