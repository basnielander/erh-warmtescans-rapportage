import { Component, input } from '@angular/core';

import { RouterModule } from '@angular/router';

export interface NavItem {
  label: string;
  route?: string;
  action?: () => void;
  icon?: string;
}

@Component({
  selector: 'app-navigation',
  imports: [RouterModule],
  templateUrl: './navigation.component.html',
  styleUrl: './navigation.component.scss'
})
export class NavigationComponent {
  navItems = input<NavItem[]>([]);

  onNavItemClick(item: NavItem, event: Event): void {
    if (item.action) {
      event.preventDefault();
      item.action();
    }
  }
}
