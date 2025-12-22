import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MapDisplayComponent } from '../map-display/map-display.component';

@Component({
  selector: 'app-report',
  standalone: true,
  imports: [CommonModule, MapDisplayComponent],
  templateUrl: './report.component.html',
  styleUrls: ['./report.component.css']
})
export class ReportComponent implements OnInit {
  folderName: string = '';
  folderId: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Get folder information from route parameters
    this.route.paramMap.subscribe(params => {
      this.folderId = params.get('folderId') || '';
      this.folderName = params.get('folderName') || '';
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }
}
