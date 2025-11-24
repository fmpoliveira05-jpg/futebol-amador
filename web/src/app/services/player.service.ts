import { inject, Injectable } from '@angular/core';
import { Player } from '../shared/Dtos/Player';
import { environment } from '../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { FilterListTeamDto } from '../shared/Dtos/Filters/FilterListTeamDto';

@Injectable({
  providedIn: 'root',
})
export class PlayerService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiURL + '/api/Player';

  /**
   *
   * @param id
   * @returns a player by id from backend
   */
  public getById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
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

  return this.http.get(`${this.apiUrl}/listTeamsToMemberShipRequest`, { params });
}
}
