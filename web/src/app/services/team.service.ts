import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { TeamDetailsDto } from '../shared/Dtos/Team/TeamDetailsDto';
import { CreateTeamDto } from '../shared/Dtos/Team/CreateTeamDto';
import { FilterListTeamDto } from '../shared/Dtos/Filters/FilterListTeamDto';
import { InfoTeamDto } from '../shared/Dtos/Team/InfoTeamDto';
import { paraHttpParams } from '../shared/http/http-params';
import { HomePageDto } from '../shared/Dtos/Home/HomePageDto';

/** Equipas: criar, consultar, editar, apagar e procurar adversários. */
@Injectable({ providedIn: 'root' })
export class TeamService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Team`;

  getTeamById(teamId: string): Observable<TeamDetailsDto> {
    return this.http.get<TeamDetailsDto>(`${this.baseUrl}/${teamId}`);
  }

  /** Resumo para a página inicial: próximos jogos e últimos resultados. */
  getHomePage(teamId: string): Observable<HomePageDto> {
    return this.http.get<HomePageDto>(`${this.baseUrl}/homeTeam/${teamId}`);
  }

  /** @returns a equipa criada, já com o id */
  createTeam(data: CreateTeamDto): Observable<CreateTeamDto> {
    return this.http.post<CreateTeamDto>(this.baseUrl, data);
  }

  updateTeam(teamId: string, data: CreateTeamDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}`, data, { responseType: 'text' as 'json' });
  }

  deleteTeam(teamId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}`);
  }

  /** Outras equipas, vistas por uma equipa (para convidar para jogos). */
  searchTeams(teamId: string, filtro?: FilterListTeamDto): Observable<InfoTeamDto[]> {
    return this.http.get<InfoTeamDto[]>(`${this.baseUrl}/${teamId}/search`, { params: paraHttpParams(filtro) });
  }
}
