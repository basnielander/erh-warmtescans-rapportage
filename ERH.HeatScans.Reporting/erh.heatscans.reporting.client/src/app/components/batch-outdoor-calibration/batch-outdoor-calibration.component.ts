import { Component, input, output, signal, computed, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImageInfo } from '../../models/image-info.model';

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
  
  temperatureMin = signal<number>(-5);
  temperatureMax = signal<number>(10);
  isProcessing = signal<boolean>(false);
  
  // Computed values - now properly reactive
  outdoorImages = computed(() => 
    this.images().filter(img => img.outdoor === true)
  );
  
  imageCount = computed(() => this.outdoorImages().length);
  
  constructor() {
    // Use effect to recalculate when images change
    effect(() => {
      
      const calibratedImages = this.outdoorImages().filter(img => 
        img.temperatureMin != null && img.temperatureMax != null
      );

      if (calibratedImages.length > 0) {
        // Use the lowest temperatureMin from all calibrated images
        const minTemp = Math.min(...calibratedImages.map(img => img.temperatureMin ?? -5));
        // Use the highest temperatureMax from all calibrated images
        const maxTemp = Math.max(...calibratedImages.map(img => img.temperatureMax ?? 10));
        
        this.temperatureMin.set(minTemp);
        this.temperatureMax.set(maxTemp);
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
