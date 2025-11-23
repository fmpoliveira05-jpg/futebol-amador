import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-player-profile-redirect',
  standalone: true,
  template: `<p>A carregar o perfil...</p>`
})
export class PlayerProfileRedirectComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  constructor() {
    const id = this.auth.getCurrentPlayerId();

    if (id) {
      this.router.navigate(['/players/details', id]);
    } else {
      this.router.navigate(['/login']);
    }
  }
}