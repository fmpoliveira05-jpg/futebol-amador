import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { MembershipRequest } from '../shared/Dtos/membership-request.model';
import { AuthService } from './auth.service';
import { validate } from 'uuid';
import { FilterMembershipRequestsPlayer } from '../shared/Dtos/Filters/FilterMembershipRequestPlayer';

@Injectable({
  providedIn: 'root',
})
export class MembershipRequestService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);

  private readonly baseUrlPlayer = `${environment.apiBaseUrl}/Player`;
  private readonly baseUrlTeam = `${environment.apiBaseUrl}/Team`;

  private readonly headers = { 'Content-Type': 'application/json' };

  formatId(Id: string): string {
    if (validate(Id)) {
      return Id;
    }

    return Id.replace(/^(\w{8})(\w{4})(\w{4})(\w{4})(\w{12})$/, '$1-$2-$3-$4-$5');
  }

  getMembershipRequestsForCurrentPlayer(
    filters?: FilterMembershipRequestsPlayer
  ): Observable<MembershipRequest[]> {
    const playerId = this.auth.getCurrentPlayerId();
    if (!playerId) {
      throw new Error('No authenticated player');
    }

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

    return this.http.get<MembershipRequest[]>(
      `${this.baseUrlPlayer}/${playerId}/membership-requests`,
      { params }
    );
  }

  acceptMembershipRequestPlayer(requestId: string): Observable<void> {
    return new Observable((observer) => {
      const playerId = this.auth.getCurrentPlayerId();
      if (!playerId) {
        observer.error('No authenticated player');
        return;
      }

      const formattedRequestId = this.formatId(requestId);

      this.http
        .post<void>(
          `${this.baseUrlPlayer}/${playerId}/membership-requests/accept`,
          JSON.stringify(formattedRequestId),
          { headers: this.headers }
        )
        .subscribe({
          next: () => {
            observer.next();
            observer.complete();
          },
          error: (err) => observer.error(err),
        });
    });
  }

  rejectMembershipRequestPlayer(requestId: string): Observable<void> {
    return new Observable((observer) => {
      const playerId = this.auth.getCurrentPlayerId();
      if (!playerId) {
        observer.error('No authenticated player');
        return;
      }

      this.http
        .delete<void>(`${this.baseUrlPlayer}/${playerId}/membership-requests/reject/${requestId}`)
        .subscribe({
          next: () => {
            observer.next();
            observer.complete();
          },
          error: (err) => observer.error(err),
        });
    });
  }

  getMembershipRequestsForCurrentTeam(): Observable<MembershipRequest[]> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          this.http
            .get<MembershipRequest[]>(`${this.baseUrlTeam}/${teamId}/membership-request`)
            .subscribe({
              next: (requests) => {
                observer.next(requests);
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }

  acceptMembershipRequestTeam(requestId: string): Observable<void> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          const formattedRequestId = this.formatId(requestId);

          this.http
            .post<void>(
              `${this.baseUrlTeam}/${teamId}/membership-request/accept`,
              JSON.stringify(formattedRequestId),
              { headers: this.headers }
            )
            .subscribe({
              next: () => {
                observer.next();
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }

  rejectMembershipRequestTeam(requestId: string): Observable<void> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          this.http
            .delete<void>(`${this.baseUrlTeam}/${teamId}/membership-request/${requestId}/reject`)
            .subscribe({
              next: () => {
                observer.next();
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }

  sendMembershipRequestPlayer(teamId: string): Observable<void> {
    return new Observable((observer) => {
      const playerId = this.auth.getCurrentPlayerId();
      if (!playerId) {
        observer.error('No authenticated player');
        return;
      }

      const formattedTeamId = this.formatId(teamId);

      this.http
        .post<void>(
          `${this.baseUrlPlayer}/${playerId}/membership-requests/send`,
          JSON.stringify(formattedTeamId),
          { headers: this.headers }
        )
        .subscribe({
          next: () => {
            observer.next();
            observer.complete();
          },
          error: (err) => {
            observer.error(err);
          },
        });
    });
  }

  sendMembershipRequestTeam(playerId: string): Observable<void> {
    return new Observable((observer) => {
      this.auth.getCurrentTeamId().subscribe({
        next: (teamId) => {
          if (!teamId) {
            observer.error('No team associated');
            return;
          }

          const formattedPlayerId = this.formatId(playerId);

          this.http
            .post<void>(
              `${this.baseUrlTeam}/${teamId}/membership-requests/send`,
              JSON.stringify(formattedPlayerId),
              { headers: this.headers }
            )
            .subscribe({
              next: () => {
                observer.next();
                observer.complete();
              },
              error: (err) => observer.error(err),
            });
        },
        error: (err) => observer.error(err),
      });
    });
  }
}
