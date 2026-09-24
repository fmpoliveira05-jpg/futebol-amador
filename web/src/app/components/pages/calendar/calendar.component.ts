import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CalendarService } from '../../../services/calendar.service';
import { AuthService } from '../../../services/auth.service';
import { CalendarDto } from '../../../shared/Dtos/Calendar/CalendarDto';
import { FilterCalendarDto } from '../../../shared/Dtos/Filters/FilterCalendarDto';
import { EstadoJogo, MATCH_RESULT, MATCH_STATUS } from '../../../shared/constants/match-status-map';
import { mensagemDeErro } from '../../../shared/http/erros';

/**
 * Calendário de jogos da equipa. Os filtros são aplicados pela API; a página mostra os jogos
 * aos poucos ("Mostrar mais") sem voltar a pedir a lista.
 */
/**
 * Ordem do calendário: primeiro os jogos por disputar (o mais próximo no topo), depois os
 * restantes do mais recente para o mais antigo.
 */
export function ordenarCalendario(jogos: CalendarDto[]): CalendarDto[] {
  const porJogar = (j: CalendarDto) => j.matchStatus === EstadoJogo.Agendado || j.matchStatus === EstadoJogo.Adiado;
  const tempo = (j: CalendarDto) => new Date(j.gameDate).getTime();
  return [...jogos].sort((a, b) => {
    if (porJogar(a) !== porJogar(b)) {
      return porJogar(a) ? -1 : 1;
    }
    return porJogar(a) ? tempo(a) - tempo(b) : tempo(b) - tempo(a);
  });
}

@Component({
  selector: 'app-calendar',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css'],
})
export class CalendarComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);
  private readonly auth = inject(AuthService);

  /** Id da equipa (parâmetro da rota). */
  readonly idTeam = input.required<string>();

  private static readonly POR_PAGINA = 10;

  protected readonly matches = signal<CalendarDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly showFilters = signal(false);
  protected readonly quantos = signal(CalendarComponent.POR_PAGINA);

  /** Filtros ligados ao formulário (só são enviados ao carregar em "Aplicar"). */
  protected filtro: FilterCalendarDto = {};

  protected readonly visibleMatches = computed(() => this.matches().slice(0, this.quantos()));
  protected readonly haMais = computed(() => this.quantos() < this.matches().length);

  protected readonly estados = MATCH_STATUS;
  protected readonly resultados = MATCH_RESULT;
  protected readonly Estado = EstadoJogo;

  ngOnInit(): void {
    this.carregar();
  }

  protected get isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  protected carregar(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.quantos.set(CalendarComponent.POR_PAGINA);
    this.calendarService.getMatchesForTeam(this.idTeam(), this.filtro).subscribe({
      next: (jogos) => {
        this.matches.set(ordenarCalendario(jogos));
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível carregar os jogos.'));
        this.isLoading.set(false);
      },
    });
  }

  protected mostrarMais(): void {
    this.quantos.update((n) => n + CalendarComponent.POR_PAGINA);
  }

  protected toggleFilters(): void {
    this.showFilters.update((v) => !v);
  }

  protected limparFiltros(): void {
    this.filtro = {};
    this.carregar();
  }

  /** Jogos ainda por jogar podem ser adiados ou cancelados. */
  /** Cor da etiqueta do estado do jogo. */
  protected classeEstado(jogo: CalendarDto): string {
    switch (jogo.matchStatus) {
      case EstadoJogo.Terminado:
        return 'badge--ok';
      case EstadoJogo.Adiado:
        return 'badge--aviso';
      case EstadoJogo.Cancelado:
        return 'badge--perigo';
      default:
        return 'badge--info';
    }
  }

  protected podeAlterar(jogo: CalendarDto): boolean {
    return jogo.matchStatus === EstadoJogo.Agendado || jogo.matchStatus === EstadoJogo.Adiado;
  }
}
