import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-logout',
  template: '',
})
export class LogoutComponent {
  private cookieService = inject(CookieService);
  
  constructor(private authService: AuthService, private router: Router) {
    this.logout();
  }

  logout(): void {
    this.cookieService.delete('access_token', '/');
    this.cookieService.delete('user_id', '/');

    this.router.navigate(['/login']);
  }
}