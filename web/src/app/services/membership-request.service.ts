import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, switchMap, throwError } from 'rxjs';
import { environment } from '../environments/environment';
import { MembershipRequest } from '../shared/Dtos/membership-request.model';
import { FilterMembershipRequestsPlayer } from '../shared/Dtos/Filters/FilterMembershipRequestPlayer';
import { paraHttpParams } from '../shared/http/http-params';
import { AuthService } from './auth.service';

/**
 * Pedidos de adesão entre jogadores e equipas.
 *
 * Um jogador sem equipa pode pedir para entrar numa equipa e uma equipa pode convidar um jogador;
 * o outro lado aceita ou rejeita. Do lado da equipa, só os administradores o podem fazer.
 */
@Injectable({ providedIn: 'root' })
export class MembershipRequestService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly baseUrlPlayer = `${environment.apiBaseUrl}/Player`;
  private readonly baseUrlTeam = `${environment.apiBaseUrl}/Team`;

  // ------------------------------------------------------------------ lado do jogador

  getMembershipRequestsForCurrentPlayer(filtros?: FilterMembershipRequestsPlayer): Observable<MembershipRequest[]> {
    return this.comJogador((playerId) =>
      this.http.get<MembershipRequest[]>(`${this.baseUrlPlayer}/${playerId}/membership-requests`, {
        params: paraHttpParams(filtros),
      })
    );
  }

  acceptMembershipRequestPlayer(requestId: string): Observable<void> {
    return this.comJogador((playerId) =>
      this.http.post<void>(`${this.baseUrlPlayer}/${playerId}/membership-requests/accept`, { requestId })
    );
  }

  rejectMembershipRequestPlayer(requestId: string): Observable<void> {
    return this.comJogador((playerId) =>
      this.http.delete<void>(`${this.baseUrlPlayer}/${playerId}/membership-requests/reject/${requestId}`)
    );
  }

  /** O jogador pede para entrar numa equipa. */
  sendMembershipRequestPlayer(teamId: string): Observable<void> {
    return this.comJogador((playerId) =>
      this.http.post<void>(`${this.baseUrlPlayer}/${playerId}/membership-requests/send`, { teamId })
    );
  }

  // ------------------------------------------------------------------ lado da equipa

  getMembershipRequestsForCurrentTeam(): Observable<MembershipRequest[]> {
    return this.comEquipa((teamId) =>
      this.http.get<MembershipRequest[]>(`${this.baseUrlTeam}/${teamId}/membership-request`)
    );
  }

  acceptMembershipRequestTeam(requestId: string): Observable<void> {
    return this.comEquipa((teamId) =>
      this.http.post<void>(`${this.baseUrlTeam}/${teamId}/membership-request/accept`, { requestId })
    );
  }

  rejectMembershipRequestTeam(requestId: string): Observable<void> {
    return this.comEquipa((teamId) =>
      this.http.delete<void>(`${this.baseUrlTeam}/${teamId}/membership-request/${requestId}/reject`)
    );
  }

  /** A equipa convida um jogador. */
  sendMembershipRequestTeam(playerId: string): Observable<void> {
    return this.comEquipa((teamId) =>
      this.http.post<void>(`${this.baseUrlTeam}/${teamId}/membership-requests/send`, { playerId })
    );
  }

  // ------------------------------------------------------------------ auxiliares

  private comJogador<T>(pedido: (playerId: string) => Observable<T>): Observable<T> {
    const playerId = this.auth.getCurrentPlayerId();
    return playerId ? pedido(playerId) : throwError(() => new Error('Sessão sem jogador autenticado.'));
  }

  private comEquipa<T>(pedido: (teamId: string) => Observable<T>): Observable<T> {
    return this.auth.getCurrentTeamId().pipe(
      switchMap((teamId) => (teamId ? pedido(teamId) : throwError(() => new Error('O utilizador não tem equipa.'))))
    );
  }
}
