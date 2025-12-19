import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { DriveBrowserComponent } from './components/drive-browser/drive-browser.component';
import { DriveItemComponent } from './components/drive-item/drive-item.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    DriveBrowserComponent,
    DriveItemComponent
  ],
  imports: [
    BrowserModule, 
    HttpClientModule,
    AppRoutingModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }

