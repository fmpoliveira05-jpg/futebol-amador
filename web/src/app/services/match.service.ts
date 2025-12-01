import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MatchDto } from '../shared/Dtos/Match/Match';
import { PostponeMatchDto } from '../shared/Dtos/Match/PostponeMatchDto';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MatchService {
  private readonly baseUrl = `${environment.apiBaseUrl}/Calendar`;

  constructor(private http: HttpClient) {}

  getMatchesForTeam(idTeam: string): Observable<MatchDto[]> {
    return this.http.get<MatchDto[]>(`${this.baseUrl}/${idTeam}`);
  }

  getMatchById(idTeam: string, idMatch: string): Observable<MatchDto> {
    return this.http.get<MatchDto>(`${this.baseUrl}/${idTeam}/${idMatch}`);
  }

  postponeMatch(idTeam: string, postponedMatch: PostponeMatchDto): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${idTeam}/PostponeMatch`, postponedMatch);
  }

  cancelMatch(idTeam: string, idMatch: string): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${idTeam}/CancelMatch/${idMatch}`);
  }
}