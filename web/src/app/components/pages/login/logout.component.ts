import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { CookieService } from 'ngx-cookie-service';

/**
 * Componente responsável pelo logout do utilizador.
 * Limpa os cookies de autenticação e redireciona para a página de login.
 */
@Component({
  selector: 'app-logout',
  template: '',
})
export class LogoutComponent {
  private cookieService = inject(CookieService);

  constructor(private authService: AuthService, private router: Router) {
    this.logout();
  }

  /**
   * Realiza o logout do utilizador, excluindo os cookies de autenticação e redirecionando para a página de login.
   */
  logout(): void {
    this.cookieService.delete('access_token', '/');
    this.cookieService.delete('user_id', '/');
    this.cookieService.delete('is_admin', '/players/details');
    this.router.navigate(['/login']);
  }
}