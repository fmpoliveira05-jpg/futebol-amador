import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { PlayerDetails, UpdatePlayerRequest } from '../shared/Dtos/player.model';
import { PlayerListItem } from '../shared/Dtos/player-list-item.model';
import { FilterListTeamDto } from '../shared/Dtos/Filters/FilterListTeamDto';
import { InfoTeamDto } from '../shared/Dtos/Team/InfoTeamDto';
import { paraHttpParams } from '../shared/http/http-params';

/** Perfis de jogador: listagem, detalhes, edição, saída da equipa e remoção da conta. */
@Injectable({ providedIn: 'root' })
export class PlayerService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Player`;

  getPlayers(): Observable<PlayerListItem[]> {
    return this.http.get<PlayerListItem[]>(`${this.baseUrl}/listPlayers`);
  }

  getPlayerById(playerId: string): Observable<PlayerDetails> {
    return this.http.get<PlayerDetails>(`${this.baseUrl}/details/${playerId}`);
  }

  getMyProfile(): Observable<PlayerDetails> {
    return this.http.get<PlayerDetails>(`${this.baseUrl}/get-my-profile`);
  }

  updatePlayer(playerId: string, data: UpdatePlayerRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/update/${playerId}`, data);
  }

  deletePlayer(playerId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${playerId}`);
  }

  leaveTeam(playerId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${playerId}/leave-team`, {});
  }

  /** Equipas a que um jogador sem equipa pode pedir para entrar. */
  searchTeams(playerId: string, filtro?: FilterListTeamDto): Observable<InfoTeamDto[]> {
    return this.http.get<InfoTeamDto[]>(`${this.baseUrl}/${playerId}/listTeamsToMemberShipRequest`, {
      params: paraHttpParams(filtro),
    });
  }
}
