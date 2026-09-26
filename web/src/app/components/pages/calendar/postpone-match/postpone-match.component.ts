import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CalendarService } from '../../../../services/calendar.service';
import { MatchDto } from '../../../../shared/Dtos/Match/MatchDto';
import { mensagemDeErro } from '../../../../shared/http/erros';

/**
 * Pedido de adiamento de um jogo: escolhe-se a nova data e o adversário recebe o pedido
 * (aceita ou rejeita na sua página de pedidos de adiamento).
 */
@Component({
  selector: 'app-postpone-match',
  imports: [FormsModule, DatePipe, RouterLink],
  templateUrl: './postpone-match.component.html',
})
export class PostponeMatchComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);
  private readonly router = inject(Router);

  readonly idTeam = input.required<string>();
  readonly idMatch = input.required<string>();

  protected readonly match = signal<MatchDto | null>(null);
  protected readonly erro = signal<string | null>(null);
  protected readonly aEnviar = signal(false);
  protected novaData = '';
  protected motivo = '';
  /** Não se pode propor uma data no passado. */
  protected readonly agora = new Date().toISOString().slice(0, 16);

  ngOnInit(): void {
    this.calendarService.getMatchById(this.idTeam(), this.idMatch()).subscribe({
      next: (jogo) => this.match.set(jogo),
      error: (err) => this.erro.set(mensagemDeErro(err, 'Não foi possível carregar o jogo.')),
    });
  }

  protected enviar(): void {
    const jogo = this.match();
    if (!jogo || !this.novaData) {
      this.erro.set('Escolhe a nova data.');
      return;
    }
    if (this.motivo.trim().length < 3) {
      this.erro.set('Indica o motivo do adiamento.');
      return;
    }
    this.aEnviar.set(true);
    this.erro.set(null);
    const adversario = jogo.team.idTeam === this.idTeam() ? jogo.opponent : jogo.team;
    this.calendarService
      .postponeMatch(this.idTeam(), {
        idMatch: jogo.idMatch,
        postPoneDate: this.novaData,
        idTeam: this.idTeam(),
        idOpponent: adversario.idTeam,
        reason: this.motivo.trim(),
      })
      .subscribe({
        next: () => this.router.navigate(['/players/calendar', this.idTeam()]),
        error: (err) => {
          this.aEnviar.set(false);
          this.erro.set(mensagemDeErro(err, 'Não foi possível pedir o adiamento.'));
        },
      });
  }
}
