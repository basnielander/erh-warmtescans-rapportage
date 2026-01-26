import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FoldersAndFileService } from '../../services/folders-and-files.service';
import { NavigationService } from '../../services/navigation.service';
import { GoogleDriveItem } from '../../models/google-drive.model';
import { NavItem } from '../navigation/navigation.component';

@Component({
  selector: 'app-folder-browser',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './folder-browser.component.html',
  styleUrl: './folder-browser.component.scss'
})
export class FolderBrowserComponent implements OnInit {
  // Use state from service
  folderStructure = computed(() => this.googleDriveService.folderStructure());
  isLoading = computed(() => this.googleDriveService.isLoading());
  error = computed(() => this.googleDriveService.error());
  hasFolderStructure = computed(() => this.googleDriveService.hasFolderStructure());
  
  // Local UI state
  expandedFolders = signal<Set<string>>(new Set());

  constructor(
    private googleDriveService: FoldersAndFileService,
    private router: Router,
    private navigationService: NavigationService
  ) {}

  ngOnInit(): void {
    this.loadFolderStructure();
    this.setupNavigation();
  }

  ngOnDestroy(): void {
    this.navigationService.clearNavItems();
  }

  setupNavigation(): void {
    const navItems: NavItem[] = [
      {
        label: 'Ververs',
        icon: '🔄',
        action: () => this.refreshFolderStructure()
      },
      {
        label: 'Alles uitklappen',
        icon: '📂',
        action: () => this.expandAll()
      },
      {
        label: 'Alles inklappen',
        icon: '📁',
        action: () => this.collapseAll()
      }
    ];
    this.navigationService.setNavItems(navItems);
  }

  async loadFolderStructure(): Promise<void> {
    // Check if folder structure is already available in state
    if (this.hasFolderStructure()) {
      console.log('Using folder structure from state');
      const structure = this.folderStructure();
      // Expand root folder by default if not already expanded
      if (structure?.id && !this.expandedFolders().has(structure.id)) {
        this.expandedFolders.update(folders => {
          const newSet = new Set(folders);
          newSet.add(structure.id);
          return newSet;
        });
      }
      return;
    }

    // Only fetch from API if state is empty
    try {
      console.log('Fetching folder structure from API');
      const data = await this.googleDriveService.getFolderStructure();
      // Expand root folder by default
      if (data?.id) {
        this.expandedFolders.update(folders => {
          const newSet = new Set(folders);
          newSet.add(data.id);
          return newSet;
        });
      }
    } catch (err: any) {
      console.error('Error loading folder structure:', err);
    }
  }

  async refreshFolderStructure(): Promise<void> {
    try {
      console.log('Fetching folder structure from API');
      const data = await this.googleDriveService.getFolderStructure();
      // Expand root folder by default
      if (data?.id) {
        this.expandedFolders.update(folders => {
          const newSet = new Set(folders);
          newSet.add(data.id);
          return newSet;
        });
      }
    } catch (err: any) {
      console.error('Error loading folder structure:', err);
    }
  }

  toggleFolder(folderId: string, item?: GoogleDriveItem, level?: number): void {
    // If this is a third-level folder (level 2, since root is 0), navigate to report page
    if (level === 2 && item?.isFolder) {
      this.navigateToReport(item);
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

  navigateToReport(folder: GoogleDriveItem): void {
    // Navigate to the report page with folder information
    this.router.navigate(['/report', folder.id, folder.name]);
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
