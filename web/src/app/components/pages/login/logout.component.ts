import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../features/players/services/auth.service';

@Component({
  selector: 'app-logout',
  template: '',
})
export class LogoutComponent {
  constructor(private authService: AuthService, private router: Router) {
    this.logout();
  }

  logout() {
    localStorage.removeItem('access_token');
    localStorage.removeItem('user_id');

    this.router.navigate(['/login']);
  }
}