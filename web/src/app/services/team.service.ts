import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';
import { TeamDetailsDto } from '../shared/Dtos/Team/TeamDetailsDto';
import { CreateTeamDto } from '../shared/Dtos/Team/CreateTeamDto';
import { FilterListTeamDto } from '../shared/Dtos/Filters/FilterListTeamDto';

/**
 * Serviço responsável pelas operações CRUD (Create, Read, Update, Delete) e pesquisa de equipas.
 */
@Injectable({
  providedIn: 'root',
})
export class TeamService {
  
  /**
   * Serviço HTTP utilizado para realizar requisições à API.
   */
  private http = inject(HttpClient);

  /**
   * URL base da API para o endpoint de Equipas.
   */
  private baseUrl = environment.apiBaseUrl + '/Team';

  /**
   * Obtém os detalhes completos de uma equipa através do seu ID.
   * @param teamId O ID da equipa a consultar.
   * @returns Um Observable com os detalhes da equipa (`TeamDetailsDto`).
   */
  getTeamById(teamId: string): Observable<TeamDetailsDto> {
    return this.http.get<TeamDetailsDto>(`${this.baseUrl}/${teamId}`);
  }

  /**
   * Cria uma nova equipa no sistema.
   * @param data DTO contendo os dados necessários para criar a equipa (nome, cidade, cores, etc.).
   * @returns Um Observable com a resposta da API (pode conter o ID da nova equipa ou void).
   */
  createTeam(data: CreateTeamDto): Observable<any>{
    return this.http.post<void>(`${this.baseUrl}`, data);
  }

  /**
   * Atualiza as informações de uma equipa existente.
   * @param teamId O ID da equipa a ser atualizada.
   * @param data DTO com os novos dados da equipa.
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  updateTeam(teamId: string, data: CreateTeamDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}`, data, {responseType: 'text' as 'json'});
  }

  /**
   * Elimina uma equipa do sistema.
   * @param teamId O ID da equipa a ser eliminada.
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  deleteTeam(teamId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}`);
  }

  /**
   * Pesquisa equipas com base em critérios de filtro.
   * @param teamId O ID da equipa atual (contexto da pesquisa).
   * @param filter (Opcional) DTO contendo os filtros de pesquisa.
   * @returns Um Observable com os resultados da pesquisa.
   */
  searchTeams(teamId: string, filter?: FilterListTeamDto): Observable<any> {
    let params = new HttpParams();

    if (filter) {
      Object.entries(filter).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          params = params.append(key, value);
        }
      });
    }

    return this.http.get(`${this.baseUrl}/${teamId}/search`, { params });
  }
}