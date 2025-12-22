import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';
import { GoogleDriveService } from './services/google-drive.service';
import { GoogleDriveBrowserComponent } from './components/google-drive-browser/google-drive-browser.component';
import { MapDisplayComponent } from './components/map-display/map-display.component';

@Component({
  selector: 'app-root',
  imports: [GoogleDriveBrowserComponent, MapDisplayComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'erh.heatscans.reporting.client';

  // Access signals from services
  user;
  isAuthenticated;
  isAuthLoading;
  authError;

  constructor(
    public authService: AuthService,
    public driveService: GoogleDriveService
  ) {
    this.user = this.authService.user;
    this.isAuthenticated = this.authService.isAuthenticated;
    this.isAuthLoading = this.authService.isLoading;
    this.authError = this.authService.error;    
  }

  async ngOnInit() {
    await this.authService.initAuth();
  }

  async onSignIn() {
    await this.authService.signIn();
  }

  async onSignOut() {
    await this.authService.signOut();
  }
}
