import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { AcceptRefusePostPoneDto, InfoPostPoneMatchDto } from '../shared/Dtos/PostPone/InfoPostPoneMatchDto';

/** Pedidos de adiamento recebidos por uma equipa, que o administrador aceita ou rejeita. */
@Injectable({ providedIn: 'root' })
export class PostponeMatchService {
  private readonly http = inject(HttpClient);

  private url(idTeam: string): string {
    return `${environment.apiBaseUrl}/Team/${idTeam}/PostPoneMatch`;
  }

  getPedidos(idTeam: string): Observable<InfoPostPoneMatchDto[]> {
    return this.http.get<InfoPostPoneMatchDto[]>(this.url(idTeam));
  }

  aceitar(idTeam: string, pedido: InfoPostPoneMatchDto): Observable<unknown> {
    return this.http.post(`${this.url(idTeam)}/AcceptPostponeMatch`, this.resposta(idTeam, pedido, 0));
  }

  rejeitar(idTeam: string, pedido: InfoPostPoneMatchDto): Observable<void> {
    return this.http.delete<void>(`${this.url(idTeam)}/RejectPostponeMatch`, {
      body: this.resposta(idTeam, pedido, 1),
    });
  }

  private resposta(idTeam: string, pedido: InfoPostPoneMatchDto, estado: 0 | 1): AcceptRefusePostPoneDto {
    const adversario = pedido.team.idTeam === idTeam ? pedido.opponent : pedido.team;
    return { idMatch: pedido.idMatch, statusPostPone: estado, idTeam, idOpponent: adversario.idTeam };
  }
}
