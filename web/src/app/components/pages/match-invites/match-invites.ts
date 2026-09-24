import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { MatchInviteService } from '../../../services/match-invite.service';
import { InfoMatchInviteDto } from '../../../shared/Dtos/Match/InfoMatchInviteDto';
import { mensagemDeErro } from '../../../shared/http/erros';

/**
 * Convites de jogo da equipa. Os convites recebidos podem ser aceites, recusados ou negociados;
 * os enviados ficam à espera da resposta do adversário.
 */
@Component({
  selector: 'app-match-invites',
  imports: [DatePipe, RouterLink],
  templateUrl: './match-invites.html',
  styleUrl: './match-invites.css',
})
export class MatchInvites implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly matchInviteService = inject(MatchInviteService);

  protected readonly teamId = signal<string | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly matchInvites = signal<InfoMatchInviteDto[]>([]);
  /** Convite com um pedido em curso (para desativar os botões dessa linha). */
  protected readonly ocupado = signal<string | null>(null);

  ngOnInit(): void {
    this.auth.getCurrentTeamId().subscribe((teamId) => {
      if (!teamId) {
        this.errorMessage.set('Não foi possível identificar a tua equipa.');
        this.isLoading.set(false);
        return;
      }
      this.teamId.set(teamId);
      this.carregar();
    });
  }

  protected recebido(convite: InfoMatchInviteDto): boolean {
    return convite.receiver.idTeam === this.teamId();
  }

  protected carregar(): void {
    const teamId = this.teamId();
    if (!teamId) {
      return;
    }
    this.isLoading.set(true);
    this.matchInviteService.getTeamMatchInvites(teamId).subscribe({
      next: (convites) => {
        this.matchInvites.set(convites);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível carregar os convites.'));
        this.isLoading.set(false);
      },
    });
  }

  accept(convite: InfoMatchInviteDto): void {
    this.responder(convite, (t) => this.matchInviteService.acceptMatchInvite(t, convite.id),
      `Jogo com ${convite.sender.name} marcado. Já está no calendário.`);
  }

  refuse(convite: InfoMatchInviteDto): void {
    if (!confirm(`Recusar o convite de ${convite.sender.name}?`)) {
      return;
    }
    this.responder(convite, (t) => this.matchInviteService.refuseMatchInvite(t, convite.id), 'Convite recusado.');
  }

  private responder(convite: InfoMatchInviteDto, pedido: (teamId: string) => Observable<void>, sucesso: string): void {
    const teamId = this.teamId();
    if (!teamId) {
      return;
    }
    this.ocupado.set(convite.id);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    pedido(teamId).subscribe({
      next: () => {
        this.ocupado.set(null);
        this.successMessage.set(sucesso);
        this.carregar();
      },
      error: (err) => {
        this.ocupado.set(null);
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível responder ao convite.'));
      },
    });
  }
}
