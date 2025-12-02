import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../..//services/auth.service';

/**
 * Guarda de autenticação responsável por proteger rotas com base em permissões de autenticação e autorização.
 * Verifica se o utilizador está autenticado e se possui as permissões necessárias para acessar a rota.
 */
@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {

  /**
   * Serviço de autenticação, utilizado para verificar se o utilizador está autenticado,
   * se ele é um administrador, ou se está associado a uma equipa.
   */
  constructor(private authService: AuthService, private router: Router) {}

  /**
   * Método que determina se a navegação para uma rota específica pode ocorrer.
   * Verifica a autenticidade do utilizador, permissões de administrador e se o utilizador tem uma equipa associada.
   * Caso alguma dessas verificações falhe, o utilizador será redirecionado para a página de login ou outra página apropriada.
   * @param route A rota ativada que está a ser verificada.
   * @param state O estado da rota, que contém informações sobre a navegação.
   * @returns Um valor booleano ou um Observable/Promise que determina se a navegação pode continuar.
   */
  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean> | Promise<boolean> | boolean {

    const isAuthenticated = this.authService.isAuthenticated();

    if (!isAuthenticated) {
      this.router.navigate(['/login']);
      return false;
    }

    if (route.data['isAdmin'] && !this.authService.isAdmin()) {
      this.router.navigate(['/']);
      return false;
    }

    if (route.data['requiresTeam'] && !this.authService.hasTeam()) {
      this.router.navigate(['/players']);
      return false;
    }

    return true;
  }
}