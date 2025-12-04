import { Component, computed, signal } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common';
import { SendMatchInviteDto } from '../../../shared/Dtos/Match/SendMatchInviteDto';
import { MatchInviteService } from '../../../services/match-invite.service';
import { InfoMatchInviteDto } from '../../../shared/Dtos/Match/InfoMatchInviteDto';

/**
 * Componente responsável pela listagem e gestão de convites de partida recebidos pela equipa.
 * Permite aceitar, recusar ou iniciar negociação de convites.
 */
@Component({
  selector: 'app-match-invites',
  imports: [CommonModule],
  templateUrl: './match-invites.html',
  styleUrl: './match-invites.css',
})
export class MatchInvites {
  teamId = signal<string | null>('');

  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly searchTerm = signal<string>('');

  matchInvites = signal<InfoMatchInviteDto[]>([]);

  constructor(private auth: AuthService, private matchInviteService: MatchInviteService) {}

  /**
   * Inicializa o componente, identificando a equipa do utilizador e carregando os seus convites.
   */
  ngOnInit(): void {
    this.auth.getCurrentTeamId().subscribe((teamId) => {
      if (!teamId) {
        this.errorMessage.set('Não foi possível identificar a tua equipa.');
        this.isLoading.set(false);
      } else {
        this.teamId.set(teamId);
        this.loadMatchInvites(teamId);
      }
    });
  }

  /**
   * Carrega a lista de convites de partida pendentes para a equipa.
   * @param teamId ID da equipa.
   */
  protected loadMatchInvites(teamId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.matchInviteService.getTeamMatchInvites(teamId).subscribe({
      next: (data) => {
        this.matchInvites.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Não foi possível carregar os convites de partida da equipa.');
        this.isLoading.set(false);
      },
    });
  }

  /**
   * Aceita um convite de partida recebido.
   * @param invite Objeto contendo os dados do convite.
   */
  accept(invite: InfoMatchInviteDto) {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    if (this.teamId()) {
      this.matchInviteService.acceptMatchInvite(this.teamId()!, invite.id).subscribe({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Não foi possível aceitar convite de partida.');
          this.isLoading.set(false);
        },
      });
    }
  }

  /**
   * Recusa um convite de partida.
   * @param invite Objeto contendo os dados do convite.
   */
  refuse(invite: InfoMatchInviteDto) {
    if (this.teamId()) {
      this.matchInviteService.refuseMatchInvite(this.teamId()!, invite.id).subscribe({
        next: () => {
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Não foi possível recusar convite de partida.');
          this.isLoading.set(false);
        },
      });
    }
  }

  /**
   * Inicia o processo de negociação (contraproposta) para um convite.
   * @param invite Objeto contendo os dados do convite.
   */
  negotiate(invite: InfoMatchInviteDto) {}
}