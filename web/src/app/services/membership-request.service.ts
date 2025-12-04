import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { MembershipRequest } from '../shared/Dtos/membership-request.model';
import { AuthService } from './auth.service';
import { validate } from 'uuid';
import { FilterMembershipRequestsPlayer } from '../shared/Dtos/Filters/FilterMembershipRequestPlayer';

/**
 * Serviço responsável pela gestão dos pedidos de adesão, tanto para jogadores quanto para equipas.
 * Inclui funções para obter, aceitar, rejeitar e enviar pedidos de adesão.
 */
@Injectable({
  providedIn: 'root',
})
export class MembershipRequestService {
  
  /**
   * Serviço HTTP utilizado para realizar requisições à API.
   */
  private readonly http = inject(HttpClient);

  /**
   * Serviço de autenticação, utilizado para acessar informações sobre o jogador e a equipa atual.
   */
  private readonly auth = inject(AuthService);

  /**
   * URL base para requisições relacionadas a jogadores.
   */
  private readonly baseUrlPlayer = `${environment.apiBaseUrl}/Player`;

  /**
   * URL base para requisições relacionadas a equipas.
   */
  private readonly baseUrlTeam = `${environment.apiBaseUrl}/Team`;

  /**
   * Cabeçalhos para as requisições HTTP, incluindo o tipo de conteúdo como JSON.
   */
  private readonly headers = { 'Content-Type': 'application/json' };

  /**
   * Formata o ID de acordo com o padrão UUID, se necessário.
   * @param Id O ID a ser formatado.
   * @returns O ID formatado no padrão UUID.
   */
  formatId(Id: string): string {
    if (validate(Id)) {
      return Id;
    }

    return Id.replace(/^(\w{8})(\w{4})(\w{4})(\w{4})(\w{12})$/, '$1-$2-$3-$4-$5');
  }

  /**
   * Adquire os pedidos de adesão do player atual.
   * @param filters Filtros de pesquisa dos pedidos.
   * @returns Lista de pedidos de adesão.
   */
  getMembershipRequestsForCurrentPlayer(
    filters?: FilterMembershipRequestsPlayer
  ): Observable<MembershipRequest[]> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    let params = new HttpParams();

    if (filters) {
      if (filters.SenderName) params = params.set('SenderName', filters.SenderName!);
      if (filters.MinDate)
        params = params.set('MinDate', filters.MinDate.toISOString().split('T')[0]);
      if (filters.MaxDate)
        params = params.set('MaxDate', filters.MaxDate.toISOString().split('T')[0]);
    
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          params = params.append(key, value);
        }
      });
    }

    return this.http.get<MembershipRequest[]>(
      `${this.baseUrlPlayer}/${playerId}/membership-requests`,
      { params }
    );
  }

  /**
   * Aceita um pedido de adesão para o jogador autenticado.
   * @param requestId O ID do pedido de adesão a ser aceito.
   * @returns Um Observable vazio após o sucesso.
   * @throws Um erro se o jogador não estiver autenticado.
   */
  acceptMembershipRequestPlayer(requestId: string): Observable<void> {
    return new Observable((observer) => {
      const playerId = this.auth.getCurrentPlayerId();
      if (!playerId) {
        observer.error('No authenticated player');
        return;
      }

      const formattedRequestId = this.formatId(requestId);

      this.http
        .post<void>(
          `${this.baseUrlPlayer}/${playerId}/membership-requests/accept`,
          JSON.stringify(formattedRequestId),
          { headers: this.headers }
        )
        .subscribe({
          next: () => {
            observer.next();
            observer.complete();
          },
          error: (err) => observer.error(err),
        });
    });
  }

  /**
   * Rejeita um pedido de adesão para o jogador autenticado.
   * @param requestId O ID do pedido de adesão a ser rejeitado.
   * @returns Um Observable vazio após o sucesso.
   * @throws Um erro se o jogador não estiver autenticado.
   */
  rejectMembershipRequestPlayer(requestId: string): Observable<void> {
    return new Observable((observer) => {
      const playerId = this.auth.getCurrentPlayerId();
      if (!playerId) {
        observer.error('No authenticated player');
        return;
      }

      this.http
        .delete<void>(`${this.baseUrlPlayer}/${playerId}/membership-requests/reject/${requestId}`)
        .subscribe({
          next: () => {
            observer.next();
            observer.complete();
          },
          error: (err) => observer.error(err),
        });
    });
  }

  /**
   * Obtém os pedidos de adesão para a equipa atual associada ao jogador autenticado.
   * @returns Um Observable que contém uma lista de pedidos de adesão.
   * @throws Um erro se não houver equipa associada ao jogador.
   */
  getMembershipRequestsForCurrentTeam(): Observable<MembershipRequest[]> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          this.http
            .get<MembershipRequest[]>(`${this.baseUrlTeam}/${teamId}/membership-request`)
            .subscribe({
              next: (requests) => {
                observer.next(requests);
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }

  /**
   * Aceita um pedido de adesão para a equipa atual.
   * @param requestId O ID do pedido de adesão a ser aceito.
   * @returns Um Observable vazio após o sucesso.
   * @throws Um erro se não houver equipa associada ao jogador.
   */
  acceptMembershipRequestTeam(requestId: string): Observable<void> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          const formattedRequestId = this.formatId(requestId);

          this.http
            .post<void>(
              `${this.baseUrlTeam}/${teamId}/membership-request/accept`,
              JSON.stringify(formattedRequestId),
              { headers: this.headers }
            )
            .subscribe({
              next: () => {
                observer.next();
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }

  /**
   * Rejeita um pedido de adesão para a equipa atual.
   * @param requestId O ID do pedido de adesão a ser rejeitado.
   * @returns Um Observable vazio após o sucesso.
   * @throws Um erro se não houver equipa associada ao jogador.
   */
  rejectMembershipRequestTeam(requestId: string): Observable<void> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          this.http
            .delete<void>(`${this.baseUrlTeam}/${teamId}/membership-request/${requestId}/reject`)
            .subscribe({
              next: () => {
                observer.next();
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }

  /**
   * Envia um pedido de adesão de jogador para a equipa especificada.
   * @param teamId O ID da equipa para a qual o jogador quer se candidatar.
   * @returns Um Observable vazio após o sucesso.
   * @throws Um erro se o jogador não estiver autenticado.
   */
  sendMembershipRequestPlayer(teamId: string): Observable<void> {
    return new Observable((observer) => {
      const playerId = this.auth.getCurrentPlayerId();
      if (!playerId) {
        observer.error('No authenticated player');
        return;
      }

      const formattedTeamId = this.formatId(teamId);

      this.http
        .post<void>(
          `${this.baseUrlPlayer}/${playerId}/membership-requests/send`,
          JSON.stringify(formattedTeamId),
          { headers: this.headers }
        )
        .subscribe({
          next: () => {
            observer.next();
            observer.complete();
          },
          error: (err) => {
            observer.error(err);
          },
        });
    });
  }

  /**
   * Envia um pedido de adesão de equipa para o jogador especificado.
   * @param playerId O ID do jogador para o qual a equipa deseja enviar um pedido de adesão.
   * @returns Um Observable vazio após o sucesso.
   * @throws Um erro se não houver equipa associada ao jogador.
   */
  sendMembershipRequestTeam(playerId: string): Observable<void> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          const formattedPlayerId = this.formatId(playerId);

          this.http
            .post<void>(
              `${this.baseUrlTeam}/${teamId}/membership-requests/send`,
              JSON.stringify(formattedPlayerId),
              { headers: this.headers }
            )
            .subscribe({
              next: () => {
                observer.next();
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }
}
