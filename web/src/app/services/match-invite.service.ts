import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../environments/environment';
import { SendMatchInviteDto } from '../shared/Dtos/Match/SendMatchInviteDto';
import { Observable } from 'rxjs';
import { InfoMatchInviteDto } from '../shared/Dtos/Match/InfoMatchInviteDto';
import { FilterMatchInviteDto } from '../shared/Dtos/Filters/FilterMatchInviteDto';

/**
 * Serviço responsável pela gestão de convites de partida (Match Invites).
 * Permite listar, enviar, aceitar, recusar e negociar convites entre equipas.
 */
@Injectable({
  providedIn: 'root',
})
export class MatchInviteService {
  
  /**
   * Serviço HTTP utilizado para realizar requisições à API.
   */
  private readonly http = inject(HttpClient);

  /**
   * Serviço de autenticação injetado (pode ser usado para obter contexto do utilizador/equipa).
   */
  private readonly auth = inject(AuthService);

  /**
   * URL base da API para os endpoints de convites de partida.
   */
  private readonly baseUrl = `${environment.apiBaseUrl}/MatchInvite`;

  /**
   * Obtém a lista de convites de partida de uma equipa específica, com possibilidade de filtragem.
   * Converte os filtros (datas, nomes) em parâmetros de consulta HTTP.
   * * @param teamId O ID da equipa para a qual se pretende obter os convites.
   * @param filters (Opcional) DTO contendo filtros como nome do remetente e intervalo de datas.
   * @returns Um Observable contendo uma lista de objetos `InfoMatchInviteDto`.
   */
  getTeamMatchInvites(
    teamId: string,
    filters?: FilterMatchInviteDto
  ): Observable<InfoMatchInviteDto[]> {
    let params = new HttpParams();

    if (filters) {
      if (filters.SenderName) params = params.set('SenderName', filters.SenderName!);
      
      // Formatação de datas para garantir o envio correto (YYYY-MM-DD)
      if (filters.MinDate)
        params = params.set('MinDate', filters.MinDate.toISOString().split('T')[0]);
      if (filters.MaxDate)
        params = params.set('MaxDate', filters.MaxDate.toISOString().split('T')[0]);

      // Adiciona quaisquer outros filtros genéricos presentes no objeto
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          params = params.append(key, value);
        }
      });
    }

    return this.http.get<InfoMatchInviteDto[]>(`${this.baseUrl}/${teamId}`, { params });
  }

  /**
   * Envia um novo convite de partida de uma equipa para outra.
   * * @param teamId O ID da equipa que está a enviar o convite.
   * @param data DTO contendo os dados do convite (adversário, data, local).
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  sendMatchInvite(teamId: string, data: SendMatchInviteDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${teamId}/match-invites`, data);
  }

  /**
   * Aceita um convite de partida pendente.
   * Envia o ID do convite no corpo da requisição para processamento no backend.
   * * @param teamId O ID da equipa que está a aceitar o convite.
   * @param idMatchInvite O ID do convite a ser aceito.
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  acceptMatchInvite(teamId: string, idMatchInvite: string): Observable<void> {
    const body = JSON.stringify(idMatchInvite);

    const params = new HttpParams().set('idTeam', teamId);
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.http.post<void>(`${this.baseUrl}/${teamId}/AcceptMatchInvite`, body, {
      headers,
      params,
    });
  }

  /**
   * Recusa um convite de partida recebido.
   * * @param teamId O ID da equipa que está a recusar o convite.
   * @param idMatchInvite O ID do convite a ser recusado (removido).
   * @returns Um Observable vazio (`void`) indicando a conclusão da operação.
   */
  refuseMatchInvite(teamId: string, idMatchInvite: string): Observable<void> {
    const body = JSON.stringify(idMatchInvite);

    const params = new HttpParams().set('idTeam', teamId);
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.http.delete<void>(`${this.baseUrl}/${teamId}/RefuseMatchInvite/`, {
      headers,
      params,
      body,
    });
  }

  /**
   * Inicia ou processa a negociação de um convite existente (contraproposta).
   */
  negotiateMatchInvite(teamId: string, data: SendMatchInviteDto): Observable<any> {
    console.log(data);
    return this.http.put<void>(`${this.baseUrl}/${teamId}/Negociate/`, data);
  }
}
