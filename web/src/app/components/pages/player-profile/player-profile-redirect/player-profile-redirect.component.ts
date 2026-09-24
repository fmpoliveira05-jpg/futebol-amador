import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../../services/auth.service';

/**
 * Componente responsável por redirecionar o utilizador para o perfil do jogador.
 * Se o ID do jogador estiver disponível, redireciona para a página de detalhes do jogador.
 * Caso contrário, redireciona para a página de login.
 */
@Component({
  selector: 'app-player-profile-redirect',
  standalone: true,
  template: `<p>A carregar o perfil...</p>`,
})
export class PlayerProfileRedirectComponent {
  private auth = inject(AuthService); // Serviço de autenticação
  private router = inject(Router); // Serviço de navegação

  constructor() {
    const id = this.auth.getCurrentPlayerId();

    if (id) {
      this.router.navigate(['/players/details', id]);
    } else {
      this.router.navigate(['/login']);
    }
  }
}