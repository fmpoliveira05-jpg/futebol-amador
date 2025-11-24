import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { PlayerDetails, UpdatePlayerRequest } from '../shared/models/player.model';
import { environment } from '../environments/environment';
import { PlayerListItem } from '../shared/models/player-list-item.model';

@Injectable({
  providedIn: 'root',
})
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
}