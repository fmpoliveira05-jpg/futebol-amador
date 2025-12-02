import { Component, computed, signal } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common';
import { SendMatchInviteDto } from '../../../shared/Dtos/Match/SendMatchInviteDto';
import { MatchInviteService } from '../../../services/match-invite.service';
import { InfoMatchInviteDto } from '../../../shared/Dtos/Match/InfoMatchInviteDto';

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

  negotiate(invite: InfoMatchInviteDto) {}
}
