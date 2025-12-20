import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleDriveService } from '../../services/google-drive.service';
import { GoogleDriveItem } from '../../models/google-drive.model';

@Component({
  selector: 'app-google-drive-browser',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './google-drive-browser.component.html',
  styleUrl: './google-drive-browser.component.css'
})
export class GoogleDriveBrowserComponent implements OnInit {
  folderStructure: GoogleDriveItem | null = null;
  isLoading = false;
  error: string | null = null;

  constructor(private googleDriveService: GoogleDriveService) {}

  ngOnInit(): void {
    this.loadFolderStructure();
  }

  loadFolderStructure(folderId?: string): void {
    this.isLoading = true;
    this.error = null;

    this.googleDriveService.getFolderStructure(folderId).subscribe({
      next: (data) => {
        this.folderStructure = data;
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load folder structure: ' + (err.message || 'Unknown error');
        this.isLoading = false;
        console.error('Error loading folder structure:', err);
      }
    });
  }

  onFolderClick(folderId: string): void {
    this.loadFolderStructure(folderId);
  }
}
