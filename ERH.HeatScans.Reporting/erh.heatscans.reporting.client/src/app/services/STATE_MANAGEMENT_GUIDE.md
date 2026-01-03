# State Management Guide

## Folder Structure State Management

The `FoldersAndFileService` now provides application-wide state management for the folder structure using Angular Signals. This allows any component in the application to access and react to changes in the folder structure.

### Available State

The service exposes the following readonly signals:

```typescript
// The complete folder structure data
folderStructure: Signal<GoogleDriveItem | null>

// Loading state
isLoading: Signal<boolean>

// Error state
error: Signal<string | null>

// Computed signal - true if folder structure has been loaded
hasFolderStructure: Signal<boolean>
```

### Usage in Components

#### Basic Access

To access the folder structure state in any component:

```typescript
import { Component } from '@angular/core';
import { FoldersAndFileService } from './services/folders-and-files.service';

@Component({
  selector: 'app-my-component',
  template: `
    @if (isLoading()) {
      <p>Loading...</p>
    } @else if (error()) {
      <p>Error: {{ error() }}</p>
    } @else if (folderStructure()) {
      <p>Root folder: {{ folderStructure()?.name }}</p>
    }
  `
})
export class MyComponent {
  // Access the state signals directly from the service
  folderStructure = computed(() => this.foldersAndFileService.folderStructure());
  isLoading = computed(() => this.foldersAndFileService.isLoading());
  error = computed(() => this.foldersAndFileService.error());
  hasFolderStructure = computed(() => this.foldersAndFileService.hasFolderStructure());

  constructor(private foldersAndFileService: FoldersAndFileService) {}
}
```

#### Best Practice: Check State Before Fetching

**Important**: Always check if the folder structure is already available in state before calling `getFolderStructure()`. This prevents unnecessary API calls:

```typescript
import { Component, OnInit, computed } from '@angular/core';
import { FoldersAndFileService } from './services/folders-and-files.service';

@Component({
  selector: 'app-folder-consumer'
})
export class FolderConsumerComponent implements OnInit {
  folderStructure = computed(() => this.foldersAndFileService.folderStructure());
  hasFolderStructure = computed(() => this.foldersAndFileService.hasFolderStructure());

  constructor(private foldersAndFileService: FoldersAndFileService) {}

  async ngOnInit(): Promise<void> {
    // Check if folder structure is already loaded
    if (this.hasFolderStructure()) {
      console.log('Using folder structure from state');
      // Use the existing state - no API call needed
      const structure = this.folderStructure();
      // ... work with the structure
      return;
    }

    // Only fetch from API if state is empty
    try {
      console.log('Fetching folder structure from API');
      await this.foldersAndFileService.getFolderStructure();
      // State is automatically updated
    } catch (error) {
      console.error('Failed to load folder structure:', error);
    }
  }
}
```

#### Reactive Updates

The signals are reactive, so any changes to the folder structure will automatically update all components that use them:

```typescript
import { Component, effect } from '@angular/core';
import { FoldersAndFileService } from './services/folders-and-files.service';

@Component({
  selector: 'app-reactive-component'
})
export class ReactiveComponent {
  constructor(private foldersAndFileService: FoldersAndFileService) {
    // React to changes in the folder structure
    effect(() => {
      const structure = this.foldersAndFileService.folderStructure();
      if (structure) {
        console.log('Folder structure updated:', structure);
        // Perform any side effects here
      }
    });
  }
}
```

#### Loading the Folder Structure

To load the folder structure, call the `getFolderStructure()` method:

```typescript
async loadData() {
  try {
    const structure = await this.foldersAndFileService.getFolderStructure();
    // The state is automatically updated
    // No need to manually set anything
  } catch (error) {
    // Error is automatically captured in the error signal
    console.error('Failed to load:', error);
  }
}
```

#### Clearing the State

To clear the folder structure state (e.g., on logout):

```typescript
this.foldersAndFileService.clearFolderStructure();
```

### Pattern Consistency

This state management pattern follows the same approach used in `AuthService`:
- Private writable signals for internal state management
- Public readonly signals for component access
- Computed signals for derived state
- Automatic state updates within service methods
- Centralized error handling

### Benefits

1. **Single Source of Truth**: The folder structure is loaded once and shared across all components
2. **Automatic Updates**: Changes propagate automatically to all components using the signals
3. **Type Safety**: Full TypeScript support with proper typing
4. **Performance**: Angular's signal system ensures efficient change detection
5. **Consistent Pattern**: Follows the same pattern as authentication state management
6. **Reduced API Calls**: By checking state before fetching, unnecessary network requests are avoided

### Example: Real-World Usage

See `FolderBrowserComponent` for a complete example of:
- Checking if state exists before making API calls
- Using computed signals to access state
- Managing local UI state alongside shared application state
