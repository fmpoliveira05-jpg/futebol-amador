import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { environment } from '../environments/environment';
import { jwtDecode } from 'jwt-decode';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}`;

  getToken(): string | null {
    return localStorage.getItem('access_token');
  }

  getCurrentPlayerId(): string | null {
    return localStorage.getItem('user_id');
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  decodeToken(): any {
    const token = this.getToken();
    if (token) {
      return jwtDecode(token);
    }
    return null;
  }

  isAdmin(): boolean {
    const decodedToken = this.decodeToken();
    return decodedToken?.isAdmin || false;
  }

  hasTeam(): boolean {
    const decodedToken = this.decodeToken();
    return decodedToken?.idTeam != null;
  }

  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/User/login`, { email, password }).pipe(
      map((response: any) => {
        localStorage.setItem('access_token', response?.firebaseLoginResponseDto?.idToken);
        localStorage.setItem('user_id', response?.firebaseLoginResponseDto?.localId);
        
        return response;
      })
    );
  }
  
  signup(
    name: string,
    email: string,
    password: string,
    dateOfBirth: string,
    address: string,
    phone: string,
    position: number,
    height: number
  ): Observable<any> {
    return this.http.post(`${this.baseUrl}/Player/create-profile`, {
      name,
      email,
      password,
      dateOfBirth,
      address,
      phone,
      position,
      height,
    }).pipe(catchError((error) => {
      return throwError(error);
    }));
  }
  
  getPlayerData(playerId: string): Observable<any> {
    return this.http.get(`${this.baseUrl}/Player/details/${playerId}`);
  }

  getCurrentTeamId(): Observable<string | null> {
    const playerId = this.getCurrentPlayerId();
    if (!playerId) {
      return of(null);
    }

    return this.getPlayerData(playerId).pipe(
      map(playerData => playerData?.idTeam ?? null)
    );
  }
}