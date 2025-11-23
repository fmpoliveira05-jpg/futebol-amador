import { inject, Injectable } from '@angular/core';
import { Player } from '../shared/models/Player';
import { environment } from '../environments/environment';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class PlayerService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiURL + '/api/Player';

  public getById(id: string): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}`);
  }

  public searchTeams(filter: any): Observable<any> {
    let params = new HttpParams();

    Object.keys(filter).forEach((key) => {
      const value = filter[key];
      if (value !== null && value !== undefined && value !== '') {
        params = params.append(key, value);
      }
    });

    return this.http.get(`${this.apiUrl}/listTeamsToMemberShipRequest`, { params });
  }
}
