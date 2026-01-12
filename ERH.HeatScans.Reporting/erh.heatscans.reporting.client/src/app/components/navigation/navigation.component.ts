import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface NavItem {
  label: string;
  route?: string;
  action?: () => void;
  icon?: string;
}

@Component({
  selector: 'app-navigation',
  imports: [CommonModule, RouterModule],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.scss'
})
export class NavigationComponent {
  @Input() navItems: NavItem[] = [];

  onNavItemClick(item: NavItem, event: Event): void {
    if (item.action) {
      event.preventDefault();
      item.action();
    }
  }
}
