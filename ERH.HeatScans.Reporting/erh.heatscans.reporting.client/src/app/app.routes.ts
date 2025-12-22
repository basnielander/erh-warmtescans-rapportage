import { Routes } from '@angular/router';
import { ReportComponent } from './components/report/report.component';
import { GoogleDriveBrowserComponent } from './components/google-drive-browser/google-drive-browser.component';

export const routes: Routes = [
  { path: '', component: GoogleDriveBrowserComponent },
  { path: 'report/:folderId/:folderName', component: ReportComponent }
];
