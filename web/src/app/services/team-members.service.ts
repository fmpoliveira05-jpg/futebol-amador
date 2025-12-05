import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { PlayerTeamDto } from '../shared/Dtos/Player/PlayerTeamDto';

/**
 * Interface que define os filtros disponíveis para a listagem de membros de uma equipa.
 */
export interface TeamMembersFilters {
  /** Filtra por estatuto de administrador (true para admins, false para membros normais). */
  isAdmin?: boolean;
  /** Filtra pelo nome do membro. */
  name?: string;
}

/**
 * Serviço responsável pela gestão dos membros de uma equipa.
 * Permite listar membros com filtros, remover jogadores e gerir permissões (promover/despromover).
 */
@Injectable({
  providedIn: 'root',
})
export class TeamMembersService {
  
  /**
   * Serviço HTTP utilizado para realizar requisições à API.
   */
  private readonly http = inject(HttpClient);

  /**
   * URL base da API para as operações relacionadas com equipas.
   */
  private readonly baseUrl = `${environment.apiBaseUrl}/Team`;

  /**
   * Obtém a lista de membros de uma equipa, permitindo filtrar por nome ou cargo.
   * @param teamId O ID da equipa.
   * @param filters Objeto contendo os critérios de filtragem (isAdmin, name).
   * @returns Um Observable com a lista de membros (`PlayerTeamDto[]`).
   */
  getTeamMembers(teamId: string, filters: TeamMembersFilters = {}): Observable<PlayerTeamDto[]> {
    let params = new HttpParams();

    if (filters.isAdmin !== undefined) {
      params = params.set('IsAdmin', String(filters.isAdmin));
    }

    if (filters.name && filters.name.trim()) {
      params = params.set('Name', filters.name.trim());
    }

    return this.http.get<PlayerTeamDto[]>(`${this.baseUrl}/${teamId}/members`, { params });
  }

  /**
   * Remove um membro específico da equipa.
   * @param teamId O ID da equipa.
   * @param playerId O ID do jogador a ser removido.
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  removeMember(teamId: string, playerId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}/members/${playerId}`);
  }

  /**
   * Promove um membro da equipa a administrador.
   * @param teamId O ID da equipa.
   * @param playerId O ID do jogador a ser promovido.
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  promoteMember(teamId: string, playerId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}/members/promote/${playerId}`, {}, {responseType: 'text' as 'json'});
  }

  /**
   * Despromove um administrador da equipa para membro normal.
   * @param teamId O ID da equipa.
   * @param adminId O ID do administrador a ser despromovido.
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  demoteAdmin(teamId: string, adminId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}/members/demote/${adminId}`, {}, {responseType: 'text' as 'json'});
  }
}