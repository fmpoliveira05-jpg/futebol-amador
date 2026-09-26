import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay } from 'rxjs';
import { environment } from '../environments/environment';
import {
  FinishMatchStateDto,
  FormationDto,
  LineupDto,
  MatchReportDto,
  ResultMatchDto,
  SaveLineupDto,
} from '../shared/Dtos/Competicao/competicao';

/** Onze inicial, resultado (com os eventos) e relatório de um jogo. */
@Injectable({ providedIn: 'root' })
export class JogoService {
  private readonly http = inject(HttpClient);
  private readonly api = environment.apiBaseUrl;

  /** As táticas não mudam: pede-se uma vez. */
  readonly taticas$: Observable<FormationDto[]> = this.http
    .get<FormationDto[]>(`${this.api}/lineups/formations`)
    .pipe(shareReplay(1));

  getOnze(matchId: string, teamId: string): Observable<LineupDto> {
    return this.http.get<LineupDto>(`${this.api}/lineups/${matchId}/${teamId}`);
  }

  guardarOnze(matchId: string, teamId: string, onze: SaveLineupDto): Observable<LineupDto> {
    return this.http.put<LineupDto>(`${this.api}/lineups/${matchId}/${teamId}`, onze);
  }

  getRelatorio(matchId: string): Observable<MatchReportDto> {
    return this.http.get<MatchReportDto>(`${this.api}/matches/${matchId}/report`);
  }

  enviarResultado(matchId: string, resultado: ResultMatchDto): Observable<FinishMatchStateDto> {
    return this.http.post<FinishMatchStateDto>(`${this.api}/matches/${matchId}/result`, resultado);
  }
}
