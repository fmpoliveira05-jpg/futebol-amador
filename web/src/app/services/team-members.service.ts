import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { TeamMember } from '../shared/Dtos/team-member.model';

export interface TeamMembersFilters {
  isAdmin?: boolean;
  name?: string;
}

@Injectable({
  providedIn: 'root',
})
export class TeamMembersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/Team`;

  getTeamMembers(teamId: string, filters: TeamMembersFilters = {}): Observable<TeamMember[]> {
    let params = new HttpParams();

    if (filters.isAdmin !== undefined) {
      params = params.set('IsAdmin', String(filters.isAdmin));
    }

    if (filters.name && filters.name.trim()) {
      params = params.set('Name', filters.name.trim());
    }

    return this.http.get<TeamMember[]>(`${this.baseUrl}/${teamId}/members`, { params });
  }

  removeMember(teamId: string, playerId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${teamId}/members/${playerId}`);
  }

  promoteMember(teamId: string, playerId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}/members/promote/${playerId}`, {}, {responseType: 'text' as 'json'});
  }

  demoteAdmin(teamId: string, adminId: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${teamId}/members/demote/${adminId}`, {}, {responseType: 'text' as 'json'});
  }
}