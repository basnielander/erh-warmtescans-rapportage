# Navigation Component

A reusable Angular navigation component that displays a 60px height navigation bar with background color #f1612b. The component supports page-specific navigation items.

## Features

- Fixed 60px height navigation bar
- Orange/red background (#f1612b)
- Support for both route links and action buttons
- Optional icons for navigation items
- Responsive design with hover effects

## Usage

### 1. Import the Component

```typescript
import { NavigationComponent, NavItem } from '../navigation/navigation.component';

@Component({
  imports: [NavigationComponent, ...],
  // ...
})
export class YourComponent {
  navItems: NavItem[] = [];
}
```

### 2. Define Navigation Items

```typescript
navItems: NavItem[] = [
  {
    label: 'Back to Folders',
    route: '/',
    icon: '←'
  },
  {
    label: 'Refresh',
    icon: '🔄',
    action: () => this.refresh()
  }
];
```

### 3. Add to Template

```html
<app-navigation [navItems]="navItems"></app-navigation>
```

## NavItem Interface

```typescript
export interface NavItem {
  label: string;       // Display text
  route?: string;      // Router link (optional)
  action?: () => void; // Click handler (optional)
  icon?: string;       // Icon/emoji (optional)
}
```

## Examples

### Folder Browser Page
- Refresh button
- Expand all folders
- Collapse all folders

### Report Page
- Back to folders link
- Save report button
- Download report button

## Styling

The navigation bar uses:
- Background: #f1612b (orange/red)
- Height: 60px
- Text color: white
- Hover effect: lighter background on items
- Max width: 1200px with auto margins for centering
