import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ImageScaleComponent } from './image-scale.component';
import { ImageScale } from '../../models/image-scale-model';
import { signal } from '@angular/core';

describe('ImageScaleComponent', () => {
  let component: ImageScaleComponent;
  let fixture: ComponentFixture<ImageScaleComponent>;

  const mockImageScale: ImageScale = {
    data: 'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==',
    mimeType: 'image/png',
    minTemperature: 15.5,
    maxTemperature: 25.3
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImageScaleComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(ImageScaleComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display maximum temperature with 1 decimal place when scale data is provided', () => {
    fixture.componentRef.setInput('imageScale', mockImageScale);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const maxTempElement = compiled.querySelector('.temperature-max');
    expect(maxTempElement?.textContent?.trim()).toBe('25.3°C');
  });

  it('should display minimum temperature with 1 decimal place when scale data is provided', () => {
    fixture.componentRef.setInput('imageScale', mockImageScale);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const minTempElement = compiled.querySelector('.temperature-min');
    expect(minTempElement?.textContent?.trim()).toBe('15.5°C');
  });

  it('should display image with correct src when scale data is provided', () => {
    fixture.componentRef.setInput('imageScale', mockImageScale);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const imgElement = compiled.querySelector('.scale-image') as HTMLImageElement;
    expect(imgElement).toBeTruthy();
    expect(imgElement.src).toContain('data:image/png;base64,');
  });

  it('should format temperature correctly', () => {
    expect(component.formatTemperature(20.5)).toBe('20.5°C');
    expect(component.formatTemperature(20)).toBe('20.0°C');
    expect(component.formatTemperature(undefined)).toBe('N/A');
    expect(component.formatTemperature(null as any)).toBe('N/A');
  });

  it('should handle null scale data gracefully', () => {
    fixture.componentRef.setInput('imageScale', null);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const emptyState = compiled.querySelector('.image-scale-empty');
    expect(emptyState).toBeTruthy();
    expect(emptyState?.textContent).toContain('No scale data');
  });

  it('should handle undefined scale data gracefully', () => {
    fixture.componentRef.setInput('imageScale', undefined);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const emptyState = compiled.querySelector('.image-scale-empty');
    expect(emptyState).toBeTruthy();
  });

  it('should handle missing temperatures gracefully', () => {
    const imageScaleWithoutTemps: ImageScale = {
      data: mockImageScale.data,
      mimeType: mockImageScale.mimeType
    };
    
    fixture.componentRef.setInput('imageScale', imageScaleWithoutTemps);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const maxTempElement = compiled.querySelector('.temperature-max');
    const minTempElement = compiled.querySelector('.temperature-min');
    
    expect(maxTempElement?.textContent?.trim()).toBe('N/A');
    expect(minTempElement?.textContent?.trim()).toBe('N/A');
  });

  it('should have correct CSS classes when scale data is valid', () => {
    fixture.componentRef.setInput('imageScale', mockImageScale);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.image-scale-container')).toBeTruthy();
    expect(compiled.querySelector('.temperature-max')).toBeTruthy();
    expect(compiled.querySelector('.temperature-min')).toBeTruthy();
    expect(compiled.querySelector('.scale-image-container')).toBeTruthy();
    expect(compiled.querySelector('.scale-image')).toBeTruthy();
  });

  it('should set correct alt text for accessibility when scale data is valid', () => {
    fixture.componentRef.setInput('imageScale', mockImageScale);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    const imgElement = compiled.querySelector('.scale-image') as HTMLImageElement;
    expect(imgElement.alt).toBe('Temperature scale from 15.5°C to 25.3°C');
  });

  it('should return null from getImageUrl when scale is null', () => {
    fixture.componentRef.setInput('imageScale', null);
    fixture.detectChanges();

    expect(component.getImageUrl()).toBeNull();
  });

  it('should return null from getImageUrl when scale data is missing', () => {
    const incompleteScale: Partial<ImageScale> = {
      mimeType: 'image/png'
      // data is missing
    };
    
    fixture.componentRef.setInput('imageScale', incompleteScale as ImageScale);
    fixture.detectChanges();

    expect(component.getImageUrl()).toBeNull();
  });

  it('should correctly identify valid scale data', () => {
    fixture.componentRef.setInput('imageScale', mockImageScale);
    fixture.detectChanges();

    expect(component.hasValidScale()).toBe(true);
  });

  it('should correctly identify invalid scale data', () => {
    fixture.componentRef.setInput('imageScale', null);
    fixture.detectChanges();

    expect(component.hasValidScale()).toBe(false);
  });

  it('should show empty state when scale has no data property', () => {
    const scaleWithoutData: Partial<ImageScale> = {
      mimeType: 'image/png',
      minTemperature: 10,
      maxTemperature: 20
    };

    fixture.componentRef.setInput('imageScale', scaleWithoutData as ImageScale);
    fixture.detectChanges();

    expect(component.hasValidScale()).toBe(false);
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('.image-scale-empty')).toBeTruthy();
  });
});
