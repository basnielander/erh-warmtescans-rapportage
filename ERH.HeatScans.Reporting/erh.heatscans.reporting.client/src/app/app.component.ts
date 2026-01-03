import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';
import { FoldersAndFileService } from './services/folders-and-files.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
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

  constructor(
    public authService: AuthService,
    public foldersAndFileService: FoldersAndFileService
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
