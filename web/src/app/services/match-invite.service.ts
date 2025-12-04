import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../environments/environment';
import { SendMatchInviteDto } from '../shared/Dtos/Match/SendMatchInviteDto';
import { Observable } from 'rxjs';
import { InfoMatchInviteDto } from '../shared/Dtos/Match/InfoMatchInviteDto';
import { FilterMatchInviteDto } from '../shared/Dtos/Filters/FilterMatchInviteDto';

@Injectable({
  providedIn: 'root',
})
export class MatchInviteService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);

  private readonly baseUrl = `${environment.apiBaseUrl}/MatchInvite`;

  getTeamMatchInvites(
    teamId: string,
    filters?: FilterMatchInviteDto
  ): Observable<InfoMatchInviteDto[]> {
    let params = new HttpParams();

    if (filters) {
      if (filters.SenderName) params = params.set('SenderName', filters.SenderName!);
      if (filters.MinDate)
        params = params.set('MinDate', filters.MinDate.toISOString().split('T')[0]);
      if (filters.MaxDate)
        params = params.set('MaxDate', filters.MaxDate.toISOString().split('T')[0]);

      Object.entries(filters).forEach(([key, value]) => {
        if (value !== null && value !== undefined && value !== '') {
          params = params.append(key, value);
        }
      });
    }

    return this.http.get<InfoMatchInviteDto[]>(`${this.baseUrl}/${teamId}`, { params });
  }

  sendMatchInvite(teamId: string, data: SendMatchInviteDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${teamId}/match-invites`, data);
  }

  acceptMatchInvite(teamId: string, idMatchInvite: string): Observable<void> {
    const body = JSON.stringify(idMatchInvite);

    const params = new HttpParams().set('idTeam', teamId);
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });

    return this.http.post<void>(`${this.baseUrl}/${teamId}/AcceptMatchInvite`, body, {
      headers,
      params,
    });
  }

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

  negotiateMatchInvite(teamId: string, data: SendMatchInviteDto) {
    return this.http.put<void>(`${this.baseUrl}/${teamId}/Negociate/`, data);
  }
}
