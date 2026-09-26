import { Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable } from 'rxjs';
import { Router, RouterLink } from '@angular/router';
import { CalendarService } from '../../../../services/calendar.service';
import { mensagemDeErro } from '../../../../shared/http/erros';

/** Cancelamento de um jogo, com o motivo (que é enviado ao adversário). */
@Component({
  selector: 'app-cancel-match',
  imports: [FormsModule, RouterLink],
  templateUrl: './cancel-match.component.html',
})
export class CancelMatchComponent {
  private readonly router = inject(Router);
  private readonly calendarService = inject(CalendarService);

  readonly idTeam = input.required<string>();
  readonly idMatch = input.required<string>();
  /** `?liga=1`: jogo da liga, que não pode ficar cancelado — é remarcado para a nova data. */
  readonly liga = input<string | undefined>();

  protected novaData = '';
  protected readonly agora = new Date().toISOString().slice(0, 16);

  protected motivo = '';
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly aEnviar = signal(false);

  confirmCancel(): void {
    if (!this.motivo.trim()) {
      this.errorMessage.set('Indica o motivo do cancelamento.');
      return;
    }
    const daLiga = !!this.liga();
    if (daLiga && !this.novaData) {
      this.errorMessage.set('Um jogo da liga tem de ser remarcado: escolhe a nova data.');
      return;
    }
    this.aEnviar.set(true);
    this.errorMessage.set(null);
    const pedido: Observable<unknown> = daLiga
      ? this.calendarService.cancelarERemarcar(this.idTeam(), this.idMatch(), this.motivo.trim(), new Date(this.novaData).toISOString())
      : this.calendarService.cancelMatch(this.idTeam(), this.idMatch(), this.motivo.trim());
    pedido.subscribe({
      next: () => this.router.navigate(['/players/calendar', this.idTeam()]),
      error: (err) => {
        this.aEnviar.set(false);
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível cancelar o jogo.'));
      },
    });
  }
}
