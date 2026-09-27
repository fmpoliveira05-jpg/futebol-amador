import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, shareReplay, throwError, timer } from 'rxjs';
import { environment } from '../environments/environment';
import {
  FixtureRoundDto,
  LeagueDto,
  SeasonDto,
  StandingsDto,
  TeamTitleDto,
} from '../shared/Dtos/Competicao/competicao';

/** Ligas, classificação, calendário da época, inscrições e títulos. */
@Injectable({ providedIn: 'root' })
export class LigaService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiBaseUrl;

  /** Tempo em que a lista de ligas (dados de referência, mudam raramente) fica em memória. */
  static readonly CACHE_LIGAS_MS = 5 * 60_000;
  private ligas$: Observable<LeagueDto[]> | null = null;

  /**
   * Lista de ligas, partilhada entre páginas durante 5 minutos (`shareReplay`): a classificação,
   * as ligas e o mercado não voltam a pedi-la. Um erro não fica em cache.
   */
  getLigas(): Observable<LeagueDto[]> {
    if (!this.ligas$) {
      this.ligas$ = this.http.get<LeagueDto[]>(`${this.api}/leagues`).pipe(
        catchError((e) => {
          this.ligas$ = null;
          return throwError(() => e);
        }),
        shareReplay({ bufferSize: 1, refCount: false, windowTime: LigaService.CACHE_LIGAS_MS })
      );
      timer(LigaService.CACHE_LIGAS_MS).subscribe(() => (this.ligas$ = null));
    }
    return this.ligas$;
  }

  /** Esquece a lista de ligas (depois de criar uma liga ou de inscrever uma equipa). */
  esquecerLigas(): void {
    this.ligas$ = null;
  }

  /** Classificação da época atual (ou da indicada). */
  getClassificacao(leagueId: string, seasonId?: string): Observable<StandingsDto> {
    const params: Record<string, string> = seasonId ? { seasonId } : {};
    return this.http.get<StandingsDto>(`${this.api}/leagues/${leagueId}/standings`, { params });
  }

  getJornadas(seasonId: string): Observable<FixtureRoundDto[]> {
    return this.http.get<FixtureRoundDto[]>(`${this.api}/leagues/seasons/${seasonId}/fixtures`);
  }

  inscrever(leagueId: string, teamId: string): Observable<SeasonDto> {
    this.esquecerLigas();
    return this.http.post<SeasonDto>(`${this.api}/leagues/${leagueId}/register/${teamId}`, {});
  }

  getTitulos(teamId: string): Observable<TeamTitleDto[]> {
    return this.http.get<TeamTitleDto[] | null>(`${this.api}/Team/${teamId}/titles`).pipe(map((t) => t ?? []));
  }
}
