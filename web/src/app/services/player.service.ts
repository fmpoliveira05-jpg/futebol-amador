import { inject, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { PlayerDetails, UpdatePlayerRequest } from '../shared/Dtos/player.model';
import { environment } from '../environments/environment';
import { PlayerListItem } from '../shared/Dtos/player-list-item.model';
import { FilterListTeamDto } from '../shared/Dtos/Filters/FilterListTeamDto';

/**
 * Serviço responsável pela gestão dos jogadores.
 * Inclui métodos para obter, atualizar, excluir e realizar outras ações relacionadas aos jogadores.
 */
@Injectable({
  providedIn: 'root',
})
export class PlayerService {
  
  /**
   * Serviço HTTP utilizado para realizar requisições à API.
   */
  private readonly http = inject(HttpClient);

  /**
   * URL base para requisições relacionadas aos jogadores.
   */
  private readonly baseUrl = `${environment.apiBaseUrl}/Player`;

  /**
   * Cabeçalhos para as requisições HTTP, especificando o tipo de conteúdo como JSON.
   */
  private readonly headers = { 'Content-Type': 'application/json' };

  /**
   * Obtém a lista de jogadores.
   * @returns Um Observable que contém uma lista de objetos `PlayerListItem` com informações resumidas dos jogadores.
   */
  getPlayers(): Observable<PlayerListItem[]> {
    return this.http.get<PlayerListItem[]>(`${this.baseUrl}/listPlayers`);
  }

  /**
   * Obtém os detalhes de um jogador pelo seu ID.
   * @param playerId O ID do jogador.
   * @returns Um Observable que contém os detalhes completos do jogador.
   */
  getPlayerById(playerId: string): Observable<PlayerDetails> {
    return this.http.get<PlayerDetails>(`${this.baseUrl}/details/${playerId}`);
  }

  /**
   * Obtém o perfil do jogador autenticado.
   * @returns Um Observable que contém os detalhes do perfil do jogador.
   */
  getMyProfile(): Observable<PlayerDetails> {
    return this.http.get<PlayerDetails>(`${this.baseUrl}/get-my-profile`);
  }

  /**
   * Atualiza os dados de um jogador.
   * @param playerId O ID do jogador.
   * @param data Os dados a serem atualizados do jogador, incluindo nome, endereço, etc.
   * @returns Um Observable vazio após a atualização.
   */
  updatePlayer(playerId: string, data: UpdatePlayerRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/update/${playerId}`, data, { headers: this.headers });
  }

  /**
   * Exclui um jogador com base no seu ID.
   * @param playerId O ID do jogador a ser excluído.
   * @returns Um Observable vazio após a exclusão.
   */
  deletePlayer(playerId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${playerId}`);
  }

  /**
   * Permite que o jogador deixe a equipa atual.
   * @param playerId O ID do jogador que deseja deixar a equipa.
   * @returns Um Observable vazio após o jogador ter deixado a equipa.
   */
  leaveTeam(playerId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${playerId}/leave-team`, {});
  }

  /**
   *
   * @param filter
   * @returns backend response for a filtered list of teams
   */
  public searchTeams(filter?: FilterListTeamDto): Observable<any> {
    let params = new HttpParams();

    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          params = params.append(key, value);
        }
      });
    }

    return this.http.get(`${this.baseUrl}/listTeamsToMemberShipRequest`, { params });
  }
}
