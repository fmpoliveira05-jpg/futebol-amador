import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { TeamLeaderboardDto } from '../shared/Dtos/Leaderboard/TeamLeaderboardDto';

/** Classificação geral das 100 melhores equipas. */
@Injectable({ providedIn: 'root' })
export class LeaderboardService {
  private readonly http = inject(HttpClient);

  getClassificacao(): Observable<TeamLeaderboardDto[]> {
    return this.http.get<TeamLeaderboardDto[]>(`${environment.apiBaseUrl}/Leaderboard`);
  }
}
