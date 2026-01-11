import { Component, input, output, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-image-edit-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './image-edit-form.component.html',
  styleUrl: './image-edit-form.component.scss'
})
export class ImageEditFormComponent {
  // Input signals
  imageId = input.required<string>();
  originalComment = input.required<string>();
  originalOutdoor = input.required<boolean>();

  // Output events
  save = output<{ comment: string, outdoor: boolean }>();
  cancel = output<void>();

  // Local state for editing
  editComment = signal<string>('');
  editOutdoor = signal<boolean>(true);

  // Computed signal to check if form values have changed
  hasChanges = computed(() => {
    return this.editComment() !== this.originalComment() || 
           this.editOutdoor() !== this.originalOutdoor();
  });

  constructor() {
    // Use effect to handle input changes
    effect(() => {
      this.editComment.set(this.originalComment());
      this.editOutdoor.set(this.originalOutdoor());
    });
  }

  onSaveClick(event: MouseEvent): void {
    event.stopPropagation();
    this.save.emit({
      comment: this.editComment(),
      outdoor: this.editOutdoor()
    });
  }

  onCancelClick(event: MouseEvent): void {
    event.stopPropagation();
    this.editComment.set(this.originalComment());
    this.editOutdoor.set(this.originalOutdoor());
    this.cancel.emit();
  }
}
