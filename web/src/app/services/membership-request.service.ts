import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { MembershipRequest } from '../shared/models/membership-request.model';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root',
})
export class MembershipRequestService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly baseUrl = `${environment.apiBaseUrl}`;

  getMembershipRequestsForCurrentPlayer(): Observable<MembershipRequest[]> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.get<MembershipRequest[]>(
      `${this.baseUrl}/Player/${playerId}/membership-requests`
    );
  }

  getMembershipRequestsForCurrentTeam(): Observable<MembershipRequest[]> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.get<MembershipRequest[]>(
      `${this.baseUrl}/Team/${playerId}/membership-requests`
    );
  }

  acceptMembershipRequestPlayer(request: { requestId: string }): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.post<void>(
      `${this.baseUrl}/Player/${playerId}/membership-requests/accept`,
      request
    );
  }

  rejectMembershipRequestPlayer(request: { requestId: string }): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.delete<void>(
      `${this.baseUrl}/Player/${playerId}/membership-requests/reject/${request.requestId}`
    );
  }

  acceptMembershipRequestTeam(request: { requestId: string }): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.post<void>(
      `${this.baseUrl}/Team/${playerId}/membership-requests/accept`,
      request
    );
  }

  rejectMembershipRequestTeam(request: { requestId: string }): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.delete<void>(
      `${this.baseUrl}/Team/${playerId}/membership-requests/reject/${request.requestId}`
    );
  }
}