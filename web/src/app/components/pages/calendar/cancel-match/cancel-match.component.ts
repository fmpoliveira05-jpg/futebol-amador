import { Component, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
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

  protected motivo = '';
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly aEnviar = signal(false);

  confirmCancel(): void {
    if (!this.motivo.trim()) {
      this.errorMessage.set('Indica o motivo do cancelamento.');
      return;
    }
    this.aEnviar.set(true);
    this.errorMessage.set(null);
    this.calendarService.cancelMatch(this.idTeam(), this.idMatch(), this.motivo.trim()).subscribe({
      next: () => this.router.navigate(['/players/calendar', this.idTeam()]),
      error: (err) => {
        this.aEnviar.set(false);
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível cancelar o jogo.'));
      },
    });
  }
}
