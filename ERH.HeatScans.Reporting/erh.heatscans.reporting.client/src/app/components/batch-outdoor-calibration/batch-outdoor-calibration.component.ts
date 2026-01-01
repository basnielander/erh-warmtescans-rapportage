import { Component, input, output, signal, computed, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImageInfo } from '../../models/image.model';

@Component({
  selector: 'app-batch-outdoor-calibration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './batch-outdoor-calibration.component.html',
  styleUrl: './batch-outdoor-calibration.component.css'
})
export class BatchOutdoorCalibrationComponent implements OnInit {
  images = input<ImageInfo[]>([]);
  
  saveCalibration = output<{ imageIds: string[], temperatureMin: number, temperatureMax: number }>();
  cancel = output<void>();
  
  temperatureMin = signal<number>(0);
  temperatureMax = signal<number>(30);
  isProcessing = signal<boolean>(false);
  
  // Computed values - now properly reactive
  outdoorImages = computed(() => 
    this.images().filter(img => img.outdoor === true)
  );
  
  imageCount = computed(() => this.outdoorImages().length);
  
  constructor() {
    // Use effect to recalculate averages when images change
    effect(() => {
      const calibratedImages = this.outdoorImages().filter(img => 
        img.temperatureMin !== undefined && img.temperatureMax !== undefined
      );
      
      if (calibratedImages.length > 0) {
        const avgMin = calibratedImages.reduce((sum, img) => sum + (img.temperatureMin ?? 0), 0) / calibratedImages.length;
        const avgMax = calibratedImages.reduce((sum, img) => sum + (img.temperatureMax ?? 30), 0) / calibratedImages.length;
        
        this.temperatureMin.set(Math.round(avgMin * 10) / 10);
        this.temperatureMax.set(Math.round(avgMax * 10) / 10);
      }
    });
  }
  
  ngOnInit(): void {
    // Initialization now handled in constructor effect
  }
  
  onSave(): void {
    const min = this.temperatureMin();
    const max = this.temperatureMax();
    
    if (min >= max) {
      alert('Minimum temperature must be less than maximum temperature');
      return;
    }
    
    if (this.outdoorImages().length === 0) {
      alert('No outdoor images to calibrate');
      return;
    }
    
    this.isProcessing.set(true);
    const imageIds = this.outdoorImages().map(img => img.id);
    
    this.saveCalibration.emit({
      imageIds,
      temperatureMin: min,
      temperatureMax: max
    });
  }
  
  onCancel(): void {
    if (this.isProcessing()) {
      return; // Don't allow cancel while processing
    }
    this.cancel.emit();
  }
  
  onMinChange(value: string): void {
    const numValue = parseFloat(value);
    if (!isNaN(numValue)) {
      this.temperatureMin.set(numValue);
    }
  }
  
  onMaxChange(value: string): void {
    const numValue = parseFloat(value);
    if (!isNaN(numValue)) {
      this.temperatureMax.set(numValue);
    }
  }
}
