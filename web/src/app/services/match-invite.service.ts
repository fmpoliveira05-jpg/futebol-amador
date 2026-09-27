import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { SendMatchInviteDto } from '../shared/Dtos/Match/SendMatchInviteDto';
import { InfoMatchInviteDto } from '../shared/Dtos/Match/InfoMatchInviteDto';
import { FilterMatchInviteDto } from '../shared/Dtos/Filters/FilterMatchInviteDto';
import { paraHttpParams } from '../shared/http/http-params';
import { comIdempotencia } from '../shared/http/idempotencia';

/**
 * Convites para jogos entre equipas. Uma equipa convida outra para uma data; a outra aceita,
 * recusa ou faz uma contraproposta (negociação), e ao aceitar o jogo passa para o calendário.
 */
@Injectable({ providedIn: 'root' })
export class MatchInviteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/MatchInvite`;

  getTeamMatchInvites(teamId: string, filtros?: FilterMatchInviteDto): Observable<InfoMatchInviteDto[]> {
    return this.http.get<InfoMatchInviteDto[]>(`${this.baseUrl}/${teamId}`, { params: paraHttpParams(filtros) });
  }

  getMatchInvite(teamId: string, idMatchInvite: string): Observable<InfoMatchInviteDto> {
    return this.http.get<InfoMatchInviteDto>(`${this.baseUrl}/${teamId}/${idMatchInvite}`);
  }

  /** `chave`: Idempotency-Key da operação (a mesma ao repetir depois de um erro de rede). */
  sendMatchInvite(teamId: string, data: SendMatchInviteDto, chave?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${teamId}/match-invites`, data, comIdempotencia(chave));
  }

  /** O corpo é o id do convite em JSON (a API recebe um `Guid` simples). */
  acceptMatchInvite(teamId: string, idMatchInvite: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${teamId}/AcceptMatchInvite`, JSON.stringify(idMatchInvite), {
      headers: { 'Content-Type': 'application/json' },
    });
  }

  refuseMatchInvite(teamId: string, idMatchInvite: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}/RefuseMatchInvite`, {
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(idMatchInvite),
    });
  }

  negotiateMatchInvite(teamId: string, data: SendMatchInviteDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}/Negociate`, data);
  }
}
