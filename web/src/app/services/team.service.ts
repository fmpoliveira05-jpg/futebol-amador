import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';
import { TeamDetailsDto } from '../shared/Dtos/Team/TeamDetailsDto';
import { UpdateTeamDto } from '../shared/Dtos/Team/UpdateTeamDto';
import { CreateTeamDto } from '../shared/Dtos/Team/CreateTeamDto';
import { FilterListTeamDto } from '../shared/Dtos/Filters/FilterListTeamDto';
import { InfoTeamDto } from '../shared/Dtos/Team/InfoTeamDto';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl + '/Team';

  getTeamById(teamId: string): Observable<TeamDetailsDto> {
    return this.http.get<TeamDetailsDto>(`${this.baseUrl}/${teamId}`);
  }

  createTeam(data: CreateTeamDto): Observable<any>{
    return this.http.post<void>(`${this.baseUrl}`, data);
  }

  updateTeam(teamId: string, data: UpdateTeamDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}`, data);
  }

  deleteTeam(teamId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}`);
  }

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
