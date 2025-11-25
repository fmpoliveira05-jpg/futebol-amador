import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';
import { InfoTeamDto } from '../shared/Dtos/Team/InfoTeamDto';
import { TeamDetailsDto } from '../shared/Dtos/Team/TeamDetailsDto';
import { UpdateTeamDto } from '../shared/Dtos/Team/UpdateTeamDto';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiBaseUrl + '/Team';

  getTeamById(teamId: string): Observable<TeamDetailsDto> {
    return this.http.get<TeamDetailsDto>(`${this.baseUrl}/${teamId}`);
  }

  updateTeam(teamId: string, data: UpdateTeamDto): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}`, data);
  }

  deleteTeam(teamId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}`);
  }
}
