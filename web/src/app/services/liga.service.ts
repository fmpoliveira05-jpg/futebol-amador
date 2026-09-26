import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
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

  getLigas(): Observable<LeagueDto[]> {
    return this.http.get<LeagueDto[]>(`${this.api}/leagues`);
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
    return this.http.post<SeasonDto>(`${this.api}/leagues/${leagueId}/register/${teamId}`, {});
  }

  getTitulos(teamId: string): Observable<TeamTitleDto[]> {
    return this.http.get<TeamTitleDto[] | null>(`${this.api}/Team/${teamId}/titles`).pipe(map((t) => t ?? []));
  }
}
