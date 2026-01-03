import { Component, output, signal, computed, OnInit, effect, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ImageInfo } from '../../models/image-info.model';

@Component({
  selector: 'app-batch-indoor-calibration',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './batch-indoor-calibration.component.html',
  styleUrl: './batch-indoor-calibration.component.scss'
})
export class BatchIndoorCalibrationComponent implements OnInit {
  images = input<ImageInfo[]>([]);
    
  saveCalibration = output<{ imageIds: string[], temperatureMin: number, temperatureMax: number }>();
  cancel = output<void>();
  
  temperatureMin = signal<number>(15);
  temperatureMax = signal<number>(25);
  isProcessing = signal<boolean>(false);
  
  // Computed values - now properly reactive
  indoorImages = computed(() => 
    this.images().filter(img => img.outdoor === false)
  );
  
  imageCount = computed(() => this.indoorImages().length);
  
  constructor() {
    // Use effect to recalculate averages when images change
    effect(() => {
      const calibratedImages = this.indoorImages().filter(img => 
        img.temperatureMin !== undefined && img.temperatureMax !== undefined
      );
      
      if (calibratedImages.length > 0) {
        // Use the lowest temperatureMin from all calibrated images
        const minTemp = Math.min(...calibratedImages.map(img => img.temperatureMin ?? 15));
        // Use the highest temperatureMax from all calibrated images
        const maxTemp = Math.max(...calibratedImages.map(img => img.temperatureMax ?? 25));

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
    
    if (this.indoorImages().length === 0) {
      alert('No indoor images to calibrate');
      return;
    }
    
    this.isProcessing.set(true);
    const imageIds = this.indoorImages().map(img => img.id);
    
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
