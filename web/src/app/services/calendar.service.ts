import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CalendarDto } from '../shared/Dtos/Calendar/CalendarDto';
import { environment } from '../environments/environment';
import { FilterCalendarDto } from '../shared/Dtos/Filters/FilterCalendarDto';
import { MatchDto } from '../shared/Dtos/Match/MatchDto';

@Injectable({
  providedIn: 'root',
})
export class CalendarService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Calendar`;

  constructor(private http: HttpClient) {}

  /**
   * Obtém os jogos para a equipa atual com filtros aplicados.
   * @param idTeam ID da equipa.
   * @param filterDto DTO com os filtros aplicados.
   * @returns Um Observable com a lista de partidas formatada.
   */
  getMatchesForTeam(idTeam: string, filterDto: FilterCalendarDto): Observable<CalendarDto[]> {
    let params = new HttpParams();

    if (filterDto.isRealized !== undefined) {
      params = params.set('IsRealized', filterDto.isRealized.toString());
    }
    if (filterDto.isRanked !== undefined) {
      params = params.set('IsRanked', filterDto.isRanked.toString());
    }
    if (filterDto.isHome !== undefined) {
      params = params.set('IsHome', filterDto.isHome.toString());
    }
    if (filterDto.minDate) {
      params = params.set('MinDate', filterDto.minDate);
    }
    if (filterDto.maxDate) {
      params = params.set('MaxDate', filterDto.maxDate);
    }
    if (filterDto.nameOpponent) {
      params = params.set('NameOpponent', filterDto.nameOpponent);
    }

    return this.http.get<CalendarDto[]>(`${this.baseUrl}/${idTeam}`, { params });
  }

  /**
   * Obtém uma partida específica pelo ID.
   * @param idTeam ID da equipa.
   * @param idMatch ID da partida.
   * @returns Um Observable com os detalhes da partida.
   */
  getMatchById(idTeam: string, idMatch: string): Observable<MatchDto> {
    return this.http.get<MatchDto>(`${this.baseUrl}/${idTeam}/${idMatch}`);
  }

  /**
   * Adia uma partida.
   * @param idTeam ID da equipa.
   * @param postponedMatch Dados da partida adiada.
   * @returns Um Observable com a resposta da operação.
   */
  postponeMatch(idTeam: string, postponedMatch: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${idTeam}/PostponeMatch`, postponedMatch);
  }

  /**
   * Cancela uma partida.
   * @param idTeam ID da equipa.
   * @param idMatch ID da partida.
   * @returns Um Observable com a resposta da operação.
   */
  cancelMatch(idTeam: string, idMatch: string, motivo: string): Observable<any> {
    const body = JSON.stringify(motivo);
    return this.http.delete<any>(`${this.baseUrl}/${idTeam}/CancelMatch/${idMatch}`, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      body: body
    });
  }
}