import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import {
  MarketFilter,
  MarketPlayerDto,
  TeamTransferOffersDto,
  TransferOfferDto,
} from '../shared/Dtos/Competicao/competicao';
import { paraHttpParams } from '../shared/http/http-params';
import { comIdempotencia } from '../shared/http/idempotencia';

/** Mercado de transferências: jogadores, listagens e propostas. */
@Injectable({ providedIn: 'root' })
export class TransferenciasService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiBaseUrl}/transfers`;

  getMercado(teamId: string, filtro: MarketFilter = {}): Observable<MarketPlayerDto[]> {
    return this.http.get<MarketPlayerDto[]>(`${this.base}/market/${teamId}`, { params: paraHttpParams(filtro) });
  }

  listar(teamId: string, playerId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/listings/${teamId}/${playerId}`, {});
  }

  retirar(teamId: string, playerId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/listings/${teamId}/${playerId}`);
  }

  proporTransferencia(teamId: string, playerId: string, message?: string): Observable<TransferOfferDto> {
    return this.http.post<TransferOfferDto>(
      `${this.base}/offers`,
      { teamId, playerId, message: message || null },
      comIdempotencia()
    );
  }

  getPropostasEquipa(teamId: string): Observable<TeamTransferOffersDto> {
    return this.http.get<TeamTransferOffersDto>(`${this.base}/offers/team/${teamId}`);
  }

  getPropostasJogador(): Observable<TransferOfferDto[]> {
    return this.http.get<TransferOfferDto[]>(`${this.base}/offers/player`);
  }

  aceitar(offerId: string): Observable<TransferOfferDto> {
    return this.http.post<TransferOfferDto>(`${this.base}/offers/${offerId}/accept`, {});
  }

  recusar(offerId: string): Observable<TransferOfferDto> {
    return this.http.post<TransferOfferDto>(`${this.base}/offers/${offerId}/reject`, {});
  }
}
