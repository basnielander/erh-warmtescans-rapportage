import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleDriveService } from '../../services/folders-and-files.service';
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
  expandedFolders: Set<string> = new Set();

  constructor(private googleDriveService: GoogleDriveService) {}

  ngOnInit(): void {
    this.loadFolderStructure();
  }

  loadFolderStructure(): void {
    this.isLoading = true;
    this.error = null;

    this.googleDriveService.getFolderStructure().subscribe({
      next: (data) => {
        this.folderStructure = data;
        // Expand root folder by default
        if (data?.id) {
          this.expandedFolders.add(data.id);
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.error = 'Failed to load folder structure: ' + (err.message || 'Unknown error');
        this.isLoading = false;
        console.error('Error loading folder structure:', err);
      }
    });
  }

  toggleFolder(folderId: string): void {
    if (this.expandedFolders.has(folderId)) {
      this.expandedFolders.delete(folderId);
    } else {
      this.expandedFolders.add(folderId);
    }
  }

  isFolderExpanded(folderId: string): boolean {
    return this.expandedFolders.has(folderId);
  }

  expandAll(): void {
    if (this.folderStructure) {
      this.expandAllRecursive(this.folderStructure);
    }
  }

  collapseAll(): void {
    this.expandedFolders.clear();
    // Keep root expanded
    if (this.folderStructure?.id) {
      this.expandedFolders.add(this.folderStructure.id);
    }
  }

  private expandAllRecursive(item: GoogleDriveItem): void {
    if (item.isFolder) {
      this.expandedFolders.add(item.id);
      if (item.children && item.children.length > 0) {
        item.children.forEach(child => this.expandAllRecursive(child));
      }
    }
  }

  formatFileSize(bytes: number | undefined): string {
    if (!bytes) return '';
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    if (bytes < 1024 * 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
    return (bytes / (1024 * 1024 * 1024)).toFixed(1) + ' GB';
  }

  getItemCount(item: GoogleDriveItem): { folders: number; files: number } {
    let folders = 0;
    let files = 0;

    if (item.children && item.children.length > 0) {
      item.children.forEach(child => {
        if (child.isFolder) {
          folders++;
        } else {
          files++;
        }
      });
    }

    return { folders, files };
  }

  getFileIcon(mimeType: string): string {
    if (mimeType.includes('image')) return '🖼️';
    if (mimeType.includes('video')) return '🎥';
    if (mimeType.includes('audio')) return '🎵';
    if (mimeType.includes('pdf')) return '📕';
    if (mimeType.includes('document') || mimeType.includes('word')) return '📝';
    if (mimeType.includes('spreadsheet') || mimeType.includes('excel')) return '📊';
    if (mimeType.includes('presentation') || mimeType.includes('powerpoint')) return '📊';
    if (mimeType.includes('zip') || mimeType.includes('compressed')) return '🗜️';
    if (mimeType.includes('text')) return '📄';
    return '📄'; // default file icon
  }
}
