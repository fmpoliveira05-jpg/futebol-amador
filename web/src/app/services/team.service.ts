import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';
import { InfoTeamDto } from '../shared/Dtos/Team/InfoTeamDto';
import { TeamDetailsDto } from '../shared/Dtos/Team/TeamDetailsDto';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiBaseUrl + '/Team';

  public getTeamById(id: string): Observable<TeamDetailsDto> {
    console.log("teamId: " + id);
    return this.http.get<TeamDetailsDto>(`${this.apiUrl}/${id}`);
  }
}
