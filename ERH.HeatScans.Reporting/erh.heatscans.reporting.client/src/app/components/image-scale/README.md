# Image Scale Component

## Overview
A standalone Angular component that displays a temperature scale image with maximum and minimum temperature labels. The component gracefully handles null or undefined scale data by showing an empty state.

## Features
- Displays maximum temperature at the top
- Shows a scale image in the middle
- Displays minimum temperature at the bottom
- Temperatures are formatted to 1 decimal place with °C
- **Handles null/undefined scale data gracefully with empty state**
- **Validates scale data before rendering**
- Fully responsive design
- Accessible with proper ARIA labels
- Supports high contrast mode

## Usage

### Import
```typescript
import { ImageScaleComponent } from './components/image-scale/image-scale.component';
```

### Basic Example
```typescript
import { Component } from '@angular/core';
import { ImageScaleComponent } from './components/image-scale/image-scale.component';
import { ImageScale } from './models/image-scale-model';

@Component({
  selector: 'app-my-component',
  standalone: true,
  imports: [ImageScaleComponent],
  template: `
    <app-image-scale [imageScale]="scaleData" />
  `
})
export class MyComponent {
  scaleData: ImageScale | null = {
    data: 'base64-encoded-image-data',
    mimeType: 'image/png',
    minTemperature: 15.5,
    maxTemperature: 25.3
  };
}
```

### Handling Null/Undefined Data
```typescript
// The component safely handles null or undefined
scaleData: ImageScale | null = null; // Shows "No scale data" empty state
```

### HTML Template
```html
<app-image-scale [imageScale]="imageScaleData" />
```

## Input Properties

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| `imageScale` | `ImageScale \| null \| undefined` | No | Object containing scale image data and temperature range. Can be null/undefined. |

## ImageScale Model

```typescript
export interface ImageScale {
    data: string;              // Base64 encoded string from C# byte[]
    mimeType: string;          // Image MIME type (e.g., 'image/png')
    minTemperature?: number;   // Minimum temperature value
    maxTemperature?: number;   // Maximum temperature value
}
```

## Behavior

### Valid Scale Data
When valid scale data is provided:
- Displays the temperature scale with max/min labels
- Shows the scale image
- All data is rendered normally

### Invalid/Missing Scale Data
When scale data is null, undefined, or missing required fields:
- Displays an empty state with a friendly message
- Shows a placeholder icon (📊)
- Text: "No scale data"
- Does not throw errors or warnings

### Validation Logic
The component considers scale data valid if:
- Scale is not null or undefined
- Scale has a `data` property (non-empty)
- Scale has a `mimeType` property (non-empty)

## Styling

The component comes with pre-styled CSS that includes:
- Rounded corners and subtle borders
- Color-coded temperature displays (warm colors for max, cool colors for min)
- Hover effects
- Empty state styling (dashed border, muted colors)
- Responsive layout for mobile devices
- High contrast mode support

### Customization
You can override the default styles by targeting the component's CSS classes in your parent component:

```css
::ng-deep app-image-scale .temperature-max {
  background-color: #your-color;
}

::ng-deep app-image-scale .image-scale-empty {
  border-color: #your-color;
}
```

## Component Classes

- `.image-scale-container` - Main container (valid data)
- `.temperature-display` - Temperature label base style
- `.temperature-max` - Maximum temperature label
- `.temperature-min` - Minimum temperature label
- `.scale-image-container` - Image wrapper
- `.scale-image` - The scale image itself
- `.image-scale-empty` - Empty state container (null/undefined data)
- `.empty-message` - Empty state message wrapper
- `.empty-icon` - Empty state icon
- `.empty-text` - Empty state text

## Example with Real Data

```typescript
async loadImageScale(imageId: string) {
  try {
    const scale = await this.imageService.getImageScale(imageId);
    this.imageScaleData = scale; // Could be null
  } catch (error) {
    this.imageScaleData = null; // Component handles this gracefully
  }
}
```

## Error Handling

The component provides multiple layers of error handling:

1. **Null/Undefined Check**: Shows empty state
2. **Missing Data**: Validates required fields
3. **Temperature Fallback**: Shows "N/A" for missing temperatures
4. **Image Validation**: Checks for data and mimeType before rendering

## Testing

The component includes comprehensive unit tests covering:
- Component creation
- Temperature formatting (1 decimal place)
- Image display
- **Null scale data handling**
- **Undefined scale data handling**
- **Invalid/incomplete scale data handling**
- Empty state rendering
- Validation logic
- CSS class presence
- Accessibility features

Run tests with:
```bash
ng test
```

## Browser Support

The component works in all modern browsers that support:
- Angular (latest version)
- CSS Grid and Flexbox
- Base64 image encoding
- Optional chaining operator (?.)

## Accessibility

- Images include descriptive `alt` text
- Temperature values are clearly labeled
- Empty state provides clear feedback
- High contrast mode supported
- Keyboard navigation compatible
- Screen reader friendly

## Notes

- The component expects Base64-encoded image data from the backend
- Temperature values are optional and will display "N/A" if not provided
- The component is standalone and doesn't require a module import
- Uses Angular signals for reactive input handling
- **Input is now optional - component can receive null/undefined**
- **Validates scale data before attempting to render**
- **Shows user-friendly empty state when data is unavailable**

## Migration Guide

If you were using `input.required<ImageScale>()`, the component is now backward compatible:

### Before (required)
```typescript
imageScale = input.required<ImageScale>();
```

### After (optional)
```typescript
imageScale = input<ImageScale | null | undefined>();
```

Your existing code will continue to work. The component simply adds null-safety.
