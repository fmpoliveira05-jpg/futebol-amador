import { Component, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { filter } from 'rxjs';
import { AuthService } from '../../../../services/auth.service';

interface Ligacao {
  rotulo: string;
  rota: string | (string | null)[];
  exato?: boolean;
}

interface Grupo {
  titulo?: string;
  ligacoes: Ligacao[];
}

/**
 * Menu lateral. As opções dependem da sessão: visitante, jogador sem equipa, membro de uma
 * equipa ou administrador. Em ecrãs pequenos o menu abre e fecha com o botão do topo.
 */
@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.css',
})
export class SidebarComponent {
  private readonly auth = inject(AuthService);

  protected readonly aberto = signal(false);

  protected readonly grupos = computed<Grupo[]>(() => {
    const s = this.auth.sessao();

    if (!s.autenticado) {
      return [
        {
          ligacoes: [
            { rotulo: 'Início', rota: '/', exato: true },
            { rotulo: 'Classificação', rota: '/leaderboard' },
            { rotulo: 'Ligas', rota: '/ligas' },
            { rotulo: 'Entrar', rota: '/login' },
            { rotulo: 'Criar conta', rota: '/signup' },
          ],
        },
      ];
    }

    const grupos: Grupo[] = [
      {
        ligacoes: [
          { rotulo: 'Início', rota: '/', exato: true },
          { rotulo: 'O meu perfil', rota: '/players/me' },
          { rotulo: 'Classificação', rota: '/leaderboard' },
          { rotulo: 'Ligas', rota: '/ligas' },
        ],
      },
    ];

    if (!s.equipaId) {
      grupos.push({
        titulo: 'Encontrar equipa',
        ligacoes: [
          { rotulo: 'Procurar equipas', rota: '/teams' },
          { rotulo: 'Convites e pedidos', rota: '/players/membership-requests' },
          { rotulo: 'Criar equipa', rota: '/createTeam' },
        ],
      });
    } else {
      const equipa: Ligacao[] = [
        { rotulo: 'A minha equipa', rota: ['/team/details', s.equipaId] },
        { rotulo: 'Calendário', rota: ['/players/calendar', s.equipaId], exato: true },
        { rotulo: 'Propostas para mim', rota: '/players/transferencias' },
      ];
      if (s.admin) {
        equipa.push(
          { rotulo: 'Membros', rota: '/team/members' },
          { rotulo: 'Transferências', rota: '/transferencias' },
          { rotulo: 'Mercado', rota: '/mercado' },
          { rotulo: 'Jogadores livres', rota: '/players', exato: true },
          { rotulo: 'Adversários', rota: '/teams' },
          { rotulo: 'Convites de jogo', rota: '/team/matchInvites' },
          { rotulo: 'Pedidos de adiamento', rota: ['/players/calendar', s.equipaId, 'postpone-requests'] }
        );
      }
      grupos.push({ titulo: 'Equipa', ligacoes: equipa });
    }

    grupos.push({
      titulo: 'Conta',
      ligacoes: [
        { rotulo: 'Definições', rota: '/settings' },
        { rotulo: 'Sair', rota: '/logout' },
      ],
    });
    return grupos;
  });

  constructor() {
    // Fecha o menu (em ecrãs pequenos) sempre que se navega para outra página.
    inject(Router)
      .events.pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntilDestroyed()
      )
      .subscribe(() => {
        this.aberto.set(false);
        this.auth.atualizarSessao();
      });
  }
}
