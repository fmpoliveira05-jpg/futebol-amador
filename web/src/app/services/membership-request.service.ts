import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { MembershipRequest } from '../shared/Dtos/membership-request.model';
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

  acceptMembershipRequestPlayer(requestId: string): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.post<void>(
      `${this.baseUrl}/${playerId}/membership-requests/accept`,
      { requestId }
    );
  }

  rejectMembershipRequestPlayer(requestId: string): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.delete<void>(
      `${this.baseUrl}/${playerId}/membership-requests/reject/${requestId}`
    );
  }

  acceptMembershipRequestTeam(requestId: string): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.post<void>(
      `${this.baseUrl}/${playerId}/Team/membership-requests/accept`,
      { requestId }
    );
  }

  rejectMembershipRequestTeam(requestId: string): Observable<void> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

    return this.http.delete<void>(
      `${this.baseUrl}/${playerId}/Team/membership-requests/reject/${requestId}`
    );
  }
}