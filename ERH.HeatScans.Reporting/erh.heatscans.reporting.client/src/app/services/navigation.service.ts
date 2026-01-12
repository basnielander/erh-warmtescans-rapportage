import { Injectable, signal } from '@angular/core';
import { NavItem } from '../components/navigation/navigation.component';

@Injectable({
  providedIn: 'root'
})
export class NavigationService {
  private navItemsSignal = signal<NavItem[]>([]);
  
  navItems = this.navItemsSignal.asReadonly();

  setNavItems(items: NavItem[]): void {
    this.navItemsSignal.set(items);
  }

  clearNavItems(): void {
    this.navItemsSignal.set([]);
  }
}
