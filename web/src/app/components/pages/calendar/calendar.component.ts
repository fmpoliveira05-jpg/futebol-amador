import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { CalendarService } from '../../../services/calendar.service';
import { CalendarDto } from '../../../shared/Dtos/Calendar/CalendarDto';
import { FilterCalendarDto } from '../../../shared/Dtos/Filters/FilterCalendarDto';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';
import { MATCH_STATUS_MAP_TONUMBER } from '../../../shared/constants/match-status-map-to-number';
import { FormsModule } from '@angular/forms';

/**
 * Componente responsável pela gestão do calendário de jogos.
 * Permite visualizar os jogos da equipa, aplicar filtros e carregar mais partidas.
 */
@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css'],
})
export class CalendarComponent {
  private readonly calendarService = inject(CalendarService);
  private readonly authService = inject(AuthService); 
  private readonly router = inject(Router); 

  protected readonly matches = signal<CalendarDto[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly showFilters = signal<boolean>(false);

  protected readonly filterDto = signal<FilterCalendarDto>(new FilterCalendarDto());
  protected readonly matchesToShow = signal<number>(10);
  protected readonly currentPage = signal<number>(1);

  protected readonly MATCH_STATUS_MAP_TONUMBER = MATCH_STATUS_MAP_TONUMBER;

  // Lista de partidas visíveis com base nos filtros e na quantidade de partidas a mostrar
  protected readonly visibleMatches = computed(() =>
    this.filteredMatches().slice(
      (this.currentPage() - 1) * this.matchesToShow(),
      this.currentPage() * this.matchesToShow()
    )
  );

  // Lista de partidas filtradas
  protected readonly filteredMatches = computed(() =>
    this.matches().filter((match) => {
      const { isRealized, isRanked, isHome, minDate, maxDate, nameOpponent } = this.filterDto();
      return (
        (isRealized !== undefined ? match.matchStatus === (isRealized ? MATCH_STATUS_MAP_TONUMBER['Done'] : MATCH_STATUS_MAP_TONUMBER['Scheduled']) : true) &&
        (isRanked !== undefined ? match.isCompetitive === isRanked : true) &&
        (isHome !== undefined ? match.isHome === isHome : true) &&
        (minDate ? new Date(match.gameDate) >= new Date(minDate) : true) &&
        (maxDate ? new Date(match.gameDate) <= new Date(maxDate) : true) &&
        (nameOpponent ? match.opponent.name.toLowerCase().includes(nameOpponent.toLowerCase()) : true)
      );
    })
  );

  ngOnInit() {
    this.authService.getCurrentTeamId().subscribe(idTeam => {
      if (idTeam) {
        this.loadMatches(idTeam);
      }
    });
  }

  // Carregar jogos
  private loadMatches(idTeam: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.calendarService.getMatchesForTeam(idTeam, this.filterDto()).subscribe({
      next: (data: CalendarDto[]) => {
        this.matches.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Não foi possível carregar os jogos.');
        this.isLoading.set(false);
      },
    });
  }

  // Carregar mais jogos ao clicar em "Carregar mais"
  protected loadMoreMatches(): void {
    this.currentPage.set(this.currentPage() + 1);
    this.authService.getCurrentTeamId().subscribe(idTeam => {
      if (idTeam) {
        this.loadMatches(idTeam);
      }
    });
  }

  // Alternar visibilidade dos filtros
  protected toggleFilters(): void {
    this.showFilters.update((value) => !value);
  }

  // Aplicar filtros aos jogos
  protected applyFilters(): void {
    this.currentPage.set(1); // Reseta a página ao aplicar os filtros
    this.authService.getCurrentTeamId().subscribe(idTeam => {
      if (idTeam) {
        this.loadMatches(idTeam); // Recarrega os jogos com os filtros aplicados
      }
    });
  }

  // Limpar resultados da pesquisa
  protected clearSearchResults(): void {
    this.filterDto.set(new FilterCalendarDto()); 
    this.matches.set([]); 
    this.currentPage.set(1);
    this.applyFilters();
  }

  // Adiar uma partida
  protected postponeMatch(match: CalendarDto): void {
    const newDate = prompt('Introduz a nova data para adiar a partida:', match.gameDate);
    if (newDate) {
      this.authService.getCurrentTeamId().subscribe(idTeam => {
        if (!idTeam) {
          alert('Não foi possível identificar a tua equipa.');
          return;
        }
        const postponedMatch = {
          idMatch: match.idMatch,
          postPoneDate: newDate,
          idTeam: idTeam,
          idOpponent: match.opponent.idTeam
        };

        this.calendarService.postponeMatch(idTeam, postponedMatch).subscribe(() => {
          alert('Partida adiada com sucesso!');
          this.loadMatches(idTeam);
        });
      });
    }
  }

  // Confirmar cancelamento de partida
  protected confirmCancel(match: CalendarDto): void {
    const motivoCancelamento = prompt(`Tens a certeza que queres cancelar a partida com ${match.opponent.name}? Qual é o motivo?`);
    if (motivoCancelamento && confirm(`Confirmas que queres cancelar a partida com ${match.opponent.name}?`)) {
      this.cancelMatch(match, motivoCancelamento);
    }
  }

  // Cancelar partida
  protected cancelMatch(match: CalendarDto, motivo: string): void {
    this.authService.getCurrentTeamId().subscribe(idTeam => {
      if (!idTeam) {
        alert('Não foi possível identificar a tua equipa.');
        return;
      }
      this.calendarService.cancelMatch(idTeam, match.idMatch, motivo).subscribe(() => {
        alert('Partida cancelada com sucesso!');
        this.loadMatches(idTeam);
      });
    });
  }

  /**
   * Verifica se o utilizador tem permissões de administrador.
   * Retorna um valor booleano que indica se o utilizador é um administrador.
   */
  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  protected goToCancelPage(match: CalendarDto): void {
    this.router.navigate([
      '/players/calendar',
      match.team.idTeam, 
      'cancel-match',
      match.idMatch       
    ]);
  }
}