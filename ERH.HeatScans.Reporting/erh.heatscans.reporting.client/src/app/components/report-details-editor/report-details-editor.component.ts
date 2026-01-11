import { Component, output, signal, OnInit, input, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Report } from '../../models/report.model';

@Component({
  selector: 'app-report-details-editor',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './report-details-editor.component.html',
  styleUrl: './report-details-editor.component.scss'
})
export class ReportDetailsEditorComponent implements OnInit {
  // Input signal instead of @Input decorator
  report = input.required<Report>();
  
  save = output<Partial<Report>>();
  cancel = output<void>();
  
  // Form fields as signals
  address = signal<string>('');
  temperature = signal<number | undefined>(undefined);
  windSpeed = signal<number | undefined>(undefined);
  windDirection = signal<string>('');
  hoursOfSunshine = signal<number | undefined>(undefined);
  frontDoorDirection = signal<string>('');
  
  isSaving = signal<boolean>(false);
  
  // Wind direction options
  windDirectionOptions = [
    { value: 'N', label: 'Noord (N)', icon: '⬆️' },
    { value: 'NE', label: 'Noordoost (NO)', icon: '↗️' },
    { value: 'E', label: 'Oost (O)', icon: '➡️' },
    { value: 'SE', label: 'Zuidoost (ZO)', icon: '↘️' },
    { value: 'S', label: 'Zuid (Z)', icon: '⬇️' },
    { value: 'SW', label: 'Zuidwest (ZW)', icon: '↙️' },
    { value: 'W', label: 'West (W)', icon: '⬅️' },
    { value: 'NW', label: 'Noordwest (NW)', icon: '↖️' }
  ];
  
  // Door direction options
  doorDirectionOptions = [
    { value: 'N', label: 'Noord (N)', icon: '⬆️' },
    { value: 'NE', label: 'Noordoost (NO)', icon: '↗️' },
    { value: 'E', label: 'Oost (O)', icon: '➡️' },
    { value: 'SE', label: 'Zuidoost (ZO)', icon: '↘️' },
    { value: 'S', label: 'Zuid (Z)', icon: '⬇️' },
    { value: 'SW', label: 'Zuidwest (ZW)', icon: '↙️' },
    { value: 'W', label: 'West (W)', icon: '⬅️' },
    { value: 'NW', label: 'Noordwest (NW)', icon: '↖️' }
  ];
  
  constructor() {
    // Use effect to initialize form fields when report input changes
    effect(() => {
      const currentReport = this.report();
      this.address.set(currentReport.address || '');
      this.temperature.set(currentReport.temperature);
      this.windSpeed.set(currentReport.windSpeed);
      this.windDirection.set(currentReport.windDirection || '');
      this.hoursOfSunshine.set(currentReport.hoursOfSunshine);
      this.frontDoorDirection.set(currentReport.frontDoorDirection || '');
    });
  }
  
  ngOnInit(): void {
    // Initialization now handled in constructor effect
  }
  
  onSave(): void {
    if (!this.isValid()) {
      alert('Please fill in all required fields correctly');
      return;
    }
    
    this.isSaving.set(true);
    
    const updatedReport: Partial<Report> = {
      address: this.address(),
      temperature: this.temperature(),
      windSpeed: this.windSpeed(),
      windDirection: this.windDirection(),
      hoursOfSunshine: this.hoursOfSunshine(),
      frontDoorDirection: this.frontDoorDirection()
    };
    
    this.save.emit(updatedReport);
  }
  
  onCancel(): void {
    if (this.isSaving()) {
      return;
    }
    this.cancel.emit();
  }
  
  isValid(): boolean {
    // Address is required
    if (!this.address() || this.address().trim() === '') {
      return false;
    }
    
    // Temperature validation (if provided, should be reasonable)
    const temp = this.temperature();
    if (temp !== undefined && (temp < -50 || temp > 60)) {
      return false;
    }
    
    // Wind speed validation (if provided, should be non-negative)
    const windSpeed = this.windSpeed();
    if (windSpeed !== undefined && windSpeed < 0) {
      return false;
    }
    
    // Hours of sunshine validation (if provided, should be between 0 and 24)
    const sunshine = this.hoursOfSunshine();
    if (sunshine !== undefined && (sunshine < 0 || sunshine > 24)) {
      return false;
    }
    
    return true;
  }
  
  onAddressChange(value: string): void {
    this.address.set(value);
  }
  
  onTemperatureChange(value: string): void {
    const numValue = parseFloat(value);
    this.temperature.set(isNaN(numValue) ? undefined : numValue);
  }
  
  onWindSpeedChange(value: string): void {
    const numValue = parseFloat(value);
    this.windSpeed.set(isNaN(numValue) ? undefined : numValue);
  }
  
  onWindDirectionChange(value: string): void {
    this.windDirection.set(value);
  }
  
  onHoursOfSunshineChange(value: string): void {
    const numValue = parseFloat(value);
    this.hoursOfSunshine.set(isNaN(numValue) ? undefined : numValue);
  }
  
  onFrontDoorDirectionChange(value: string): void {
    this.frontDoorDirection.set(value);
  }
}
