import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../..//services/auth.service';

/**
 * Componente responsável pela exibição da barra lateral (sidebar).
 * Exibe opções de navegação baseadas no estado de autenticação e permissões do utilizador.
 */
@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css'],
})
export class SidebarComponent {
  
  /**
   * Serviço de autenticação utilizado para verificar o estado de login e permissões do utilizador.
   */
  private authService = inject(AuthService);

  /**
   * Verifica se o utilizador está autenticado.
   * Retorna um valor booleano que indica se o utilizador está autenticado ou não.
   */
  get isAuthenticated(): boolean {
    return this.authService.isAuthenticated();
  }

  /**
   * Verifica se o utilizador tem permissões de administrador.
   * Retorna um valor booleano que indica se o utilizador é um administrador.
   */
  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  /**
   * Verifica se o utilizador está associado a uma equipa.
   * Retorna um valor booleano que indica se o utilizador tem uma equipa associada.
   */
  get hasTeam(): boolean {
    return this.authService.hasTeam();
  }
}