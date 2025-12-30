import { Routes } from '@angular/router';
import { ReportComponent } from './components/report/report.component';
import { FolderBrowserComponent } from './components/folder-browser/folder-browser.component';

export const routes: Routes = [
  { path: '', component: FolderBrowserComponent },
  { path: 'report/:folderId/:folderName', component: ReportComponent }
];
