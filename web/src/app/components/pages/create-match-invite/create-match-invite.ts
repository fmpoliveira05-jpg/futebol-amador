import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { MatchInviteService } from '../../../services/match-invite.service';
import { SendMatchInviteDto } from '../../../shared/Dtos/Match/SendMatchInviteDto';
import { InfoMatchInviteDto } from '../../../shared/Dtos/Match/InfoMatchInviteDto';
import { mensagemDeErro } from '../../../shared/http/erros';
import { TeamService } from '../../../services/team.service';
import { TeamDetailsDto } from '../../../shared/Dtos/Team/TeamDetailsDto';

/**
 * Enviar um convite de jogo a outra equipa, ou fazer uma contraproposta a um convite recebido.
 *
 * A contraproposta é identificada no URL (`?convite=<id>`), para continuar a funcionar se a
 * página for recarregada.
 */
@Component({
  selector: 'app-create-match-invite',
  imports: [ReactiveFormsModule, DatePipe, RouterLink],
  templateUrl: './create-match-invite.html',
})
export class CreateMatchInvite implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly matchInviteService = inject(MatchInviteService);
  private readonly teams = inject(TeamService);

  /** Equipa adversária (parâmetro da rota). */
  readonly teamId = input.required<string>();
  /** Convite a negociar (parâmetro de query, opcional). */
  readonly convite = input<string>();

  protected readonly currentTeamId = signal<string | null>(null);
  protected readonly original = signal<InfoMatchInviteDto | null>(null);
  protected readonly adversario = signal<TeamDetailsDto | null>(null);
  protected readonly isNegotiating = computed(() => !!this.convite());
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  /** Um jogo tem de ser marcado com pelo menos 12 horas de antecedência (regra da API). */
  protected readonly minDate = CreateMatchInvite.dataMinima();

  protected readonly form = this.fb.nonNullable.group({
    gameDate: ['', [Validators.required]],
    homePitch: [true],
  });

  ngOnInit(): void {
    this.teams.getTeamById(this.teamId()).subscribe({ next: (t) => this.adversario.set(t), error: () => {} });
    this.auth.getCurrentTeamId().subscribe((id) => {
      this.currentTeamId.set(id);
      const idConvite = this.convite();
      if (id && idConvite) {
        this.matchInviteService.getMatchInvite(id, idConvite).subscribe({
          next: (c) => this.original.set(c),
          error: (err) => this.errorMessage.set(mensagemDeErro(err, 'Não foi possível carregar o convite.')),
        });
      }
    });
  }

  private static dataMinima(): string {
    const data = new Date(Date.now() + 12 * 60 * 60 * 1000);
    return new Date(data.getTime() - data.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  }

  submitAction(): void {
    const minhaEquipa = this.currentTeamId();
    if (this.form.invalid || !minhaEquipa) {
      this.form.markAllAsTouched();
      return;
    }
    const { gameDate, homePitch } = this.form.getRawValue();
    const payload: SendMatchInviteDto = {
      idSender: minhaEquipa,
      idReceiver: this.teamId(),
      gameDate,
      homePitch,
    };
    this.isLoading.set(true);
    this.errorMessage.set(null);
    const pedido = this.isNegotiating()
      ? this.matchInviteService.negotiateMatchInvite(minhaEquipa, payload)
      : this.matchInviteService.sendMatchInvite(minhaEquipa, payload);
    pedido.subscribe({
      next: () => this.router.navigate(['/team/matchInvites']),
      error: (err) => {
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível enviar o convite.'));
        this.isLoading.set(false);
      },
    });
  }
}
