import { Component, input, output, signal, HostListener, ElementRef } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-user',
  imports: [],
  templateUrl: './user.component.html',
  styleUrl: './user.component.scss'
})
export class UserComponent {
  user;
  showModal = signal(false);

  constructor(
    public authService: AuthService,
    private elementRef: ElementRef
  ) {
    this.user = this.authService.user;    
  }

  toggleModal() {
    this.showModal.update(value => !value);
  }

  async onSignOut() {
    await this.authService.signOut();
    this.showModal.set(false);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (this.showModal()) {
      const clickedInside = this.elementRef.nativeElement.contains(event.target);
      if (!clickedInside) {
        this.showModal.set(false);
      }
    }
  }
}
