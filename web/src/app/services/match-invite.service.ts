import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { environment } from '../environments/environment';
import { SendMatchInviteDto } from '../shared/Dtos/Match/SendMatchInviteDto';
import { Observable } from 'rxjs';
import { InfoMatchInviteDto } from '../shared/Dtos/Match/InfoMatchInviteDto';

@Injectable({
  providedIn: 'root',
})
export class MatchInviteService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);

  private readonly baseUrl = `${environment.apiBaseUrl}/MatchInvite`;

  private readonly headers = { 'Content-Type': 'application/json' };

  getTeamMatchInvites(teamId: string): Observable<InfoMatchInviteDto[]> {
    return this.http.get<InfoMatchInviteDto[]>(`${this.baseUrl}/${teamId}/match-invites`);
  }

  sendMatchInvites(teamId: string, data: SendMatchInviteDto): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${teamId}/match-invites`, data);
  }

  acceptMatchInvite() {}

  refuseMatchInvite() {}

  negotiateMatchInvite() {}
}
