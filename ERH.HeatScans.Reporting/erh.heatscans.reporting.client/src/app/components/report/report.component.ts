import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MapDisplayComponent } from '../map-display/map-display.component';
import { GoogleDriveService } from '../../services/folders-and-files.service';
import { Report } from '../../models/report.model';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, MapDisplayComponent],
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
  folderName: string = '';
  folderId: string = '';
  isSettingUp: boolean = false;
  setupError: string | null = null;
  
  // Signal to store the address report
  addressReport = signal<Report | null>(null);
  isLoadingReport: boolean = false;
  reportError: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private driveService: GoogleDriveService
  ) {}

  ngOnInit(): void {
    // Get folder information from route parameters
    this.route.paramMap.subscribe(params => {
      this.folderId = params.get('folderId') || '';
      this.folderName = params.get('folderName') || '';
      
      // Call the setup API when the component is initialized
      if (this.folderId) {
        this.setupAddressFolder();
      }
    });
  }

  setupAddressFolder(): void {
    this.isSettingUp = true;
    this.setupError = null;

    this.driveService.setupAddressFolder(this.folderId).subscribe({
      next: () => {
        console.log('Address folder setup completed successfully');
        this.isSettingUp = false;
        // Call getReport after successful setup
        this.loadReport();
      },
      error: (err) => {
        console.error('Error setting up address folder:', err);
        this.setupError = 'Failed to setup address folder. Please try again.';
        this.isSettingUp = false;
      }
    });
  }

  loadReport(): void {
    this.isLoadingReport = true;
    this.reportError = null;

    this.driveService.getReport(this.folderId).subscribe({
      next: (report) => {
        console.log('Report loaded successfully:', report);
        this.addressReport.set(report);
        this.isLoadingReport = false;
      },
      error: (err) => {
        console.error('Error loading report:', err);
        this.reportError = 'Failed to load report. Please try again.';
        this.isLoadingReport = false;
      }
    });
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
