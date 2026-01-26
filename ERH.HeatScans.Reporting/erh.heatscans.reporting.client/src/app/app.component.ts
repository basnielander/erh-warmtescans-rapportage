import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';
import { FoldersAndFileService } from './services/folders-and-files.service';
import { NavigationService } from './services/navigation.service';
import { UserComponent } from './components/user/user.component';
import { NavigationComponent } from './components/navigation/navigation.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, UserComponent, NavigationComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'erh.heatscans.reporting.client';

  // Access signals from services
  user;
  isAuthenticated;
  isAuthLoading;
  authError;
  navItems;

  constructor(
    public authService: AuthService,
    public foldersAndFileService: FoldersAndFileService,
    public navigationService: NavigationService
  ) {
    this.user = this.authService.user;
    this.isAuthenticated = this.authService.isAuthenticated;
    this.isAuthLoading = this.authService.isLoading;
    this.authError = this.authService.error;
    this.navItems = this.navigationService.navItems;
  }

  async ngOnInit() {
    await this.authService.initAuth();
  }

  async onSignIn() {
    await this.authService.signIn();
  }  
}
