import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleDriveService } from '../../services/folders-and-files.service';
import { GoogleDriveItem } from '../../models/google-drive.model';
import { MapDisplayComponent } from '../map-display/map-display.component';

@Component({
  selector: 'app-google-drive-browser',
  standalone: true,
  imports: [CommonModule, MapDisplayComponent],
  templateUrl: './google-drive-browser.component.html',
  styleUrl: './google-drive-browser.component.css'
})
export class GoogleDriveBrowserComponent implements OnInit {
  // State signals
  folderStructure = signal<GoogleDriveItem | null>(null);
  isLoading = signal<boolean>(false);
  error = signal<string | null>(null);
  expandedFolders = signal<Set<string>>(new Set());
  selectedThirdLevelFolder = signal<GoogleDriveItem | null>(null);

  // Computed signal for selected folder address
  selectedFolderAddress = computed(() => {
    const folder = this.selectedThirdLevelFolder();
    return folder?.name ?? '';
  });

  constructor(private googleDriveService: GoogleDriveService) {}

  ngOnInit(): void {
    this.loadFolderStructure();
  }

  loadFolderStructure(): void {
    this.isLoading.set(true);
    this.error.set(null);

    this.googleDriveService.getFolderStructure().subscribe({
      next: (data) => {
        this.folderStructure.set(data);
        // Expand root folder by default
        if (data?.id) {
          this.expandedFolders.update(folders => {
            const newSet = new Set(folders);
            newSet.add(data.id);
            return newSet;
          });
        }
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Failed to load folder structure: ' + (err.message || 'Unknown error'));
        this.isLoading.set(false);
        console.error('Error loading folder structure:', err);
      }
    });
  }

  toggleFolder(folderId: string, item?: GoogleDriveItem, level?: number): void {
    // If this is a third-level folder (level 2, since root is 0), handle it specially
    if (level === 2 && item?.isFolder) {
      this.onThirdLevelFolderClick(item);
      return;
    }

    // Normal expand/collapse behavior
    this.expandedFolders.update(folders => {
      const newSet = new Set(folders);
      if (newSet.has(folderId)) {
        newSet.delete(folderId);
      } else {
        newSet.add(folderId);
      }
      return newSet;
    });
  }

  onThirdLevelFolderClick(folder: GoogleDriveItem): void {
    // Toggle selection
    const currentSelection = this.selectedThirdLevelFolder();
    if (currentSelection?.id === folder.id) {
      this.selectedThirdLevelFolder.set(null);
    } else {
      this.selectedThirdLevelFolder.set(folder);
    }
  }

  isThirdLevelFolderSelected(folderId: string): boolean {
    return this.selectedThirdLevelFolder()?.id === folderId;
  }

  clearSelection(): void {
    this.selectedThirdLevelFolder.set(null);
  }

  isFolderExpanded(folderId: string): boolean {
    return this.expandedFolders().has(folderId);
  }

  expandAll(): void {
    const structure = this.folderStructure();
    if (structure) {
      this.expandAllRecursive(structure);
    }
  }

  collapseAll(): void {
    const rootId = this.folderStructure()?.id;
    this.expandedFolders.set(rootId ? new Set([rootId]) : new Set());
  }

  private expandAllRecursive(item: GoogleDriveItem): void {
    if (item.isFolder) {
      this.expandedFolders.update(folders => {
        const newSet = new Set(folders);
        newSet.add(item.id);
        return newSet;
      });
      
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
