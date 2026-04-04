import { Component, input, output } from '@angular/core';

import { ImageSpot } from '../../models/image-spot.model';

@Component({
  selector: 'app-temperature-spots-list',
  standalone: true,
  imports: [],
  templateUrl: './temperature-spots-list.component.html',
  styleUrl: './temperature-spots-list.component.scss'
})
export class TemperatureSpotsListComponent {
  spots = input.required<ImageSpot[]>();
  
  deleteSpot = output<string>();

  onDeleteSpotClick(event: MouseEvent, spotName: string): void {
    event.stopPropagation();
    this.deleteSpot.emit(spotName);
  }
}
