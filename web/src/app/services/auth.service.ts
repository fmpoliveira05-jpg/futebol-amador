import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { catchError, map, Observable, of, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { CookieService } from 'ngx-cookie-service';
import { environment } from '../environments/environment';
import { PlayerDetails } from '../shared/Dtos/player.model';

/** Resposta do `POST /User/login` (só os campos usados pela aplicação). */
export interface LoginResponse {
  name: string;
  idTeam?: string | null;
  isAdmin?: boolean | null;
  firebaseLoginResponseDto: {
    idToken: string;
    localId: string;
    expiresIn: string;
  };
}

/** Estado da sessão que os componentes (menu, página inicial) acompanham. */
export interface EstadoSessao {
  autenticado: boolean;
  jogadorId: string | null;
  equipaId: string | null;
  admin: boolean;
}

/** Dados enviados no registo de um jogador (`POST /Player/create-profile`). */
export interface SignupRequest {
  name: string;
  email: string;
  password: string;
  dateOfBirth: string;
  address: string;
  phone: string;
  position: number;
  height: number;
}

/**
 * Sessão do utilizador: login, registo, logout e as informações guardadas no browser (token,
 * id do jogador, id da equipa e se é administrador).
 *
 * A sessão fica em cookies com `path=/` e `SameSite=Strict`. O token do Firebase expira ao fim
 * de uma hora; `isAuthenticated()` verifica essa validade, e o interceptor termina a sessão quando
 * a API responde 401.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly cookies = inject(CookieService);
  private readonly baseUrl = environment.apiBaseUrl;

  static readonly TOKEN = 'access_token';
  static readonly USER = 'user_id';
  static readonly ADMIN = 'is_admin';
  static readonly TEAM = 'team_id';

  /** Margem, em segundos, para considerar o token expirado um pouco antes da hora. */
  private static readonly MARGEM_EXPIRACAO = 30;

  private readonly _sessao = signal<EstadoSessao>(this.lerSessao());

  /** Estado atual da sessão, como signal: muda no login, no logout e quando a equipa muda. */
  readonly sessao = this._sessao.asReadonly();
  readonly autenticado = computed(() => this.sessao().autenticado);

  getToken(): string | null {
    return this.cookies.get(AuthService.TOKEN) || null;
  }

  getCurrentPlayerId(): string | null {
    return this.cookies.get(AuthService.USER) || null;
  }

  /** @returns se existe um token e se ainda não expirou */
  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }
    try {
      const { exp } = jwtDecode<{ exp?: number }>(token);
      return !exp || exp - AuthService.MARGEM_EXPIRACAO > Date.now() / 1000;
    } catch {
      return false;
    }
  }

  isAdmin(): boolean {
    return this.cookies.get(AuthService.ADMIN) === 'true';
  }

  hasTeam(): boolean {
    return !!this.cookies.get(AuthService.TEAM);
  }

  /** @returns se o utilizador pertence a uma equipa mas não a administra */
  isMember(): boolean {
    return this.hasTeam() && !this.isAdmin();
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/User/login`, { email, password }).pipe(
      tap((response) => {
        this.guardar(AuthService.TOKEN, response.firebaseLoginResponseDto.idToken);
        this.guardar(AuthService.USER, response.firebaseLoginResponseDto.localId);
        this.setTeam(response.idTeam ?? null, response.isAdmin === true);
      })
    );
  }

  signup(dados: SignupRequest): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/Player/create-profile`, dados);
  }

  /**
   * Termina a sessão no servidor (revoga os tokens do Firebase) e apaga os dados do browser.
   * A sessão local é apagada mesmo que o pedido ao servidor falhe.
   */
  logout(): Observable<void> {
    if (!this.getToken()) {
      this.clearSession();
      return of(undefined);
    }
    return this.http.get<void>(`${this.baseUrl}/User/logout`).pipe(
      catchError(() => of(undefined)),
      tap(() => this.clearSession()),
      map(() => undefined)
    );
  }

  /** Apaga todos os dados da sessão guardados no browser. */
  clearSession(): void {
    for (const chave of [AuthService.TOKEN, AuthService.USER, AuthService.ADMIN, AuthService.TEAM]) {
      this.cookies.delete(chave, '/');
    }
    this.atualizarSessao();
  }

  /** Atualiza a equipa e o papel do utilizador (depois de criar, sair ou apagar uma equipa). */
  setTeam(teamId: string | null, isAdmin: boolean): void {
    if (teamId) {
      this.guardar(AuthService.TEAM, teamId);
    } else {
      this.cookies.delete(AuthService.TEAM, '/');
    }
    this.guardar(AuthService.ADMIN, String(teamId !== null && isAdmin));
    this.atualizarSessao();
  }

  /** Volta a ler os cookies (por exemplo, quando o token expira entretanto). */
  atualizarSessao(): void {
    this._sessao.set(this.lerSessao());
  }

  private lerSessao(): EstadoSessao {
    const autenticado = this.isAuthenticated();
    return {
      autenticado,
      jogadorId: autenticado ? this.getCurrentPlayerId() : null,
      equipaId: autenticado ? this.cookies.get(AuthService.TEAM) || null : null,
      admin: autenticado && this.isAdmin(),
    };
  }

  /**
   * Obtém os detalhes de um jogador. Se for o utilizador autenticado, aproveita para atualizar
   * a equipa e o papel guardados na sessão.
   */
  getPlayerData(playerId: string): Observable<PlayerDetails> {
    return this.http.get<PlayerDetails>(`${this.baseUrl}/Player/details/${playerId}`).pipe(
      tap((data) => {
        if (playerId === this.getCurrentPlayerId()) {
          this.setTeam(data.team?.idTeam ?? null, data.isAdmin === true);
        }
      })
    );
  }

  /** @returns o id da equipa do utilizador (da sessão ou, se não estiver guardado, da API) */
  getCurrentTeamId(): Observable<string | null> {
    const playerId = this.getCurrentPlayerId();
    if (!playerId) {
      return of(null);
    }
    const guardado = this.cookies.get(AuthService.TEAM);
    if (guardado) {
      return of(guardado);
    }
    return this.getPlayerData(playerId).pipe(
      map((data) => data.team?.idTeam ?? null),
      catchError(() => of(null))
    );
  }

  /** Confirma na API se o utilizador é administrador da sua equipa (usado pelo guard). */
  canUserActivateAdminRoute(): Observable<boolean> {
    const playerId = this.getCurrentPlayerId();
    if (!playerId) {
      return of(false);
    }
    return this.getPlayerData(playerId).pipe(
      map((data) => data.isAdmin === true && !!data.team?.idTeam),
      catchError(() => of(false))
    );
  }

  private guardar(chave: string, valor: string): void {
    const seguro = typeof location !== 'undefined' && location.protocol === 'https:';
    this.cookies.set(chave, valor, { expires: 7, path: '/', sameSite: 'Strict', secure: seguro });
  }
}
