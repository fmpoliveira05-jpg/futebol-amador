import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { CalendarDto } from '../shared/Dtos/Calendar/CalendarDto';
import { FilterCalendarDto } from '../shared/Dtos/Filters/FilterCalendarDto';
import { MatchDto } from '../shared/Dtos/Match/MatchDto';
import { PostponeMatchDto } from '../shared/Dtos/Match/PostponeMatchDto';
import { paraHttpParams } from '../shared/http/http-params';
import { CalendarMarkerDto } from '../shared/Dtos/Competicao/competicao';

/** Calendário de jogos de uma equipa: consultar, pedir adiamento e cancelar. */
@Injectable({ providedIn: 'root' })
export class CalendarService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Calendar`;

  /** Os nomes dos filtros seguem os da API (que usa `IsRanqued`). */
  getMatchesForTeam(idTeam: string, filtro: FilterCalendarDto = {}): Observable<CalendarDto[]> {
    const params = paraHttpParams(filtro, {
      isRealized: 'IsRealized',
      isRanked: 'IsRanqued',
      isHome: 'IsHome',
      minDate: 'MinDate',
      maxDate: 'MaxDate',
      nameOpponent: 'NameOpponent',
    });
    return this.http.get<CalendarDto[]>(`${this.baseUrl}/${idTeam}`, { params });
  }

  getMatchById(idTeam: string, idMatch: string): Observable<MatchDto> {
    return this.http.get<MatchDto>(`${this.baseUrl}/${idTeam}/${idMatch}`);
  }

  /** Pede ao adversário para adiar o jogo; o adversário aceita ou rejeita na página de adiamentos. */
  postponeMatch(idTeam: string, pedido: PostponeMatchDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${idTeam}/PostponeMatch`, pedido);
  }

  /** Cancelamentos com nova data e adiamentos aceites, na data original. */
  getHistorico(idTeam: string): Observable<CalendarMarkerDto[]> {
    return this.http.get<CalendarMarkerDto[]>(`${this.baseUrl}/${idTeam}/history`);
  }

  /** Jogos da liga: cancela e remarca logo para a nova data. */
  cancelarERemarcar(idTeam: string, idMatch: string, reason: string, newDate: string): Observable<MatchDto> {
    return this.http.put<MatchDto>(`${this.baseUrl}/${idTeam}/${idMatch}/cancel-reschedule`, { reason, newDate });
  }

  /** O corpo é o motivo, em JSON (a API recebe uma `string` simples). */
  cancelMatch(idTeam: string, idMatch: string, motivo: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${idTeam}/CancelMatch/${idMatch}`, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(motivo),
    });
  }
}
