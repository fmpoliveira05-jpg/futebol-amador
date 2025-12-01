import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CalendarDto } from '../shared/Dtos/Calendar/CalendarDto';
import { MatchDto } from '../shared/Dtos/Match/MatchDto';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CalendarService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Calendar`;

  constructor(private http: HttpClient) {}

   /**
   * Obtém os jogos para a equipa atual.
   * @param idTeam ID da equipa.
   * @returns Um Observable com a lista de partidas formatada.
   */
  getMatchesForTeam(idTeam: string): Observable<CalendarDto[]> {
    return this.http.get<CalendarDto[]>(`${this.baseUrl}/${idTeam}`);
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
  cancelMatch(idTeam: string, idMatch: string): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${idTeam}/CancelMatch/${idMatch}`);
  }
}