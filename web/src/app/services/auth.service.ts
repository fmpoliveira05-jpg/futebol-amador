import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of, throwError } from 'rxjs';
import { environment } from '../environments/environment';
import { jwtDecode } from 'jwt-decode';
import { CookieService } from 'ngx-cookie-service';
import { PlayerDetails } from '../shared/Dtos/player.model';

/**
 * Serviço responsável pela autenticação e gestão de sessões do utilizador.
 * Realiza login, registo, decodificação de token, verificação de permissões, entre outras funções relacionadas.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  /**
   * Serviço HTTP utilizado para realizar requisições à API.
   */
  private readonly http = inject(HttpClient);

  /**
   * URL base da API, definida a partir das configurações do ambiente.
   */
  private readonly baseUrl = `${environment.apiBaseUrl}`;

  /**
   * Serviço para manipulação de cookies.
   */
  private cookieService = inject(CookieService);

  /**
   * Obtém o token de acesso armazenado nos cookies.
   * @returns O token de acesso ou `null` se não encontrado.
   */
  getToken(): string | null {
    return this.cookieService.get('access_token');
  }

  /**
   * Obtém o ID do jogador atual armazenado nos cookies.
   * @returns O ID do jogador ou `null` se não encontrado.
   */
  getCurrentPlayerId(): string | null {
    return this.cookieService.get('user_id');
  }

  /**
   * Verifica se o utilizador está autenticado (se existe um token de acesso válido).
   * @returns `true` se o utilizador estiver autenticado, caso contrário `false`.
   */
  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  /**
   * Decodifica o token de acesso JWT e retorna o seu conteúdo.
   * @returns O conteúdo do token decodificado ou `null` se o token não existir.
   */
  decodeToken(): any {
    const token = this.getToken();
    if (token) {
      return jwtDecode(token);
    }
    return null;
  }

  /**
   * Verifica se o utilizador tem permissões de administrador.
   * @returns `true` se o utilizador for administrador, caso contrário `false`.
   */
  isAdmin(): boolean {
    return this.cookieService.get('is_admin') === 'true';
  }  
  
  /**
   * Verifica se o utilizador tem permissões de administrador.
   * @returns `true` se o utilizador for administrador, caso contrário `false`.
   */
  isMember(): boolean {
    return this.hasTeam() && this.cookieService.get('is_admin') === 'false';
  }

  /**
   * Verifica se o utilizador está associado a uma equipa.
   * @returns `true` se o utilizador tiver uma equipa associada, caso contrário `false`.
   */
  hasTeam(): boolean {
    return this.cookieService.check('team_id');
  }
  // hasTeam(): boolean {
  //   return this.getCurrentTeamId != null;
  // }

  /**
   * Realiza o login do utilizador utilizando email e senha.
   * Se o login for bem-sucedido, os cookies de acesso e ID do utilizador são configurados.
   * @param email O email do utilizador.
   * @param password A senha do utilizador.
   * @returns Um Observable com a resposta da API.
   */
  login(email: string, password: string): Observable<any> {
    return this.http.post(`${this.baseUrl}/User/login`, { email, password }).pipe(
      map((response: any) => {
        const idToken = response?.firebaseLoginResponseDto?.idToken;
        const localId = response?.firebaseLoginResponseDto?.localId;
        const isAdmin = response?.isAdmin;

        this.cookieService.set('access_token', idToken, 7, '/');
        this.cookieService.set('user_id', localId, 7, '/');
        // this.cookieService.set('is_admin', String(isAdmin), 7, '/players/details');
        this.cookieService.set('is_admin', String(isAdmin), 7, '/');

        return response;
      })
    );
  }

  /**
   * Realiza o registo de um novo jogador.
   * @param name Nome do jogador.
   * @param email Email do jogador.
   * @param password Senha do jogador.
   * @param dateOfBirth Data de nascimento do jogador.
   * @param address Endereço do jogador.
   * @param phone Número de telefone do jogador.
   * @param position Posição do jogador.
   * @param height Altura do jogador.
   * @returns Um Observable com a resposta da API ou erro.
   */
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
    return this.http
      .post(`${this.baseUrl}/Player/create-profile`, {
        name,
        email,
        password,
        dateOfBirth,
        address,
        phone,
        position,
        height,
      })
      .pipe(
        catchError((error) => {
          return throwError(error);
        })
      );
  }

  /**
   * Obtém os dados detalhados de um jogador pelo seu ID.
   * @param playerId ID do jogador.
   * @returns Um Observable com os detalhes do jogador.
   */
  getPlayerData(playerId: string): Observable<PlayerDetails> {
    // return this.http.get<PlayerDetails>(`${this.baseUrl}/Player/details/${playerId}`);
    return this.http.get<PlayerDetails>(`${this.baseUrl}/Player/details/${playerId}`).pipe(
        // Side-effect: Sempre que carregarmos os dados, atualizamos os cookies para garantir sincronia
        map(data => {
            if (data.team?.idTeam) {
                this.cookieService.set('team_id', data.team.idTeam, 7, '/');
            }
            // Atualiza o admin também por segurança
            this.cookieService.set('is_admin', String(data.isAdmin), 7, '/');
            return data;
        })
    );
  }

  /**
   * Obtém o ID da equipa do jogador atual.
   * @returns Um Observable com o ID da equipa do jogador ou `null` se não houver equipa associada.
   */
  getCurrentTeamId(): Observable<string | null> {
    // const playerId = this.getCurrentPlayerId();
    // if (!playerId) {
    //   return of(null);
    // }

    // return this.getPlayerData(playerId).pipe(
    //   map((playerData) => playerData?.team?.idTeam ?? null),
    //   catchError((error) => {
    //     console.error('Erro ao buscar dados do jogador:', error);
    //     return of(null);
    //   })
    // );
    const playerId = this.getCurrentPlayerId();
    if (!playerId) return of(null);

    // Tenta pegar do cookie primeiro para ser rápido
    const cachedTeamId = this.cookieService.get('team_id');
    if (cachedTeamId) return of(cachedTeamId);

    return this.getPlayerData(playerId).pipe(
      map((playerData) => playerData?.team?.idTeam ?? null),
      catchError((error) => {
        console.error('Erro ao buscar dados do jogador:', error);
        return of(null);
      })
    );
  }

  /**
   * @method canUserActivateAdminRoute
   * Obtém o ID do jogador atual e verifica, via API, se o utilizador possui permissões de administrador.
   * Este método é o ponto de verificação assíncrono para o AuthGuard.
   * @returns Um Observable que emite true se o utilizador for Admin, ou false caso contrário.
   */
  canUserActivateAdminRoute(): Observable<boolean> {
    const playerId = this.getCurrentPlayerId();
    if (!playerId) {
      return of(false);
    }

    return this.getPlayerData(playerId).pipe(
      map((playerData) => playerData?.isAdmin === true),
      catchError(() => of(false))
    );
  }
}
