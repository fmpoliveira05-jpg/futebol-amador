import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { environment } from '../environments/environment';
import { PlayerDetails } from '../shared/Dtos/player.model';

/** Resposta do `POST /User/login` (só os campos usados pela aplicação). Os tokens ficam em cookies HttpOnly. */
export interface LoginResponse {
  name: string;
  idTeam?: string | null;
  isAdmin?: boolean | null;
  firebaseLoginResponseDto: {
    localId: string;
    expiresIn: string;
  };
}

/** Resposta do registo quando é preciso confirmar o e-mail antes de entrar. */
export interface RegistoPendente {
  playerId: string;
  verificacaoEmailPendente: true;
  mensagem: string;
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
  /** Campo-armadilha: tem de ir vazio (só um robô o preenche). */
  website?: string;
}

/** Dados guardados no browser: só indicações para a interface, nunca tokens. */
interface SessaoGuardada {
  jogadorId: string;
  equipaId: string | null;
  admin: boolean;
}

/** Cabeçalho com o token do Cloudflare Turnstile (quando a página o tem). */
export const CABECALHO_TURNSTILE = 'X-Turnstile-Token';

/**
 * Sessão do utilizador: login, registo, logout e renovação.
 *
 * Os tokens do Firebase ficam em cookies `HttpOnly` definidos pela API (`__Host-fa_session` e
 * `__Secure-fa_refresh`): o JavaScript nunca os vê. No browser só se guarda, em `localStorage`, o
 * id do jogador, a equipa e se é administrador — servem para a interface (menu, guard) e não dão
 * acesso a nada: é a API que decide. Quando a API responde 401, o interceptor tenta renovar a
 * sessão uma vez (`renovarSessao`) antes de mandar para o login.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  static readonly CHAVE = 'fa_sessao';

  private readonly _sessao = signal<EstadoSessao>(this.lerSessao());
  private renovacao$: Observable<boolean> | null = null;

  /** Estado atual da sessão, como signal: muda no login, no logout e quando a equipa muda. */
  readonly sessao = this._sessao.asReadonly();
  readonly autenticado = computed(() => this.sessao().autenticado);

  getCurrentPlayerId(): string | null {
    return this.lerGuardada()?.jogadorId ?? null;
  }

  /** @returns se o browser tem indicação de sessão (a validade real é confirmada pela API). */
  isAuthenticated(): boolean {
    return this.lerGuardada() !== null;
  }

  isAdmin(): boolean {
    const s = this.lerGuardada();
    return !!s && s.admin && !!s.equipaId;
  }

  hasTeam(): boolean {
    return !!this.lerGuardada()?.equipaId;
  }

  /** @returns se o utilizador pertence a uma equipa mas não a administra */
  isMember(): boolean {
    return this.hasTeam() && !this.isAdmin();
  }

  login(email: string, password: string, turnstile?: string | null, website = ''): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/User/login`, { email, password, website }, { headers: this.cabecalhos(turnstile) })
      .pipe(tap((response) => this.aceitarSessao(response)));
  }

  /**
   * Cria a conta. Normalmente a resposta é um registo pendente (confirmar o e-mail); se a API não
   * exigir a confirmação, traz logo a sessão (cookies) e fica-se autenticado.
   */
  signup(dados: SignupRequest, turnstile?: string | null): Observable<RegistoPendente | LoginResponse> {
    return this.http
      .post<RegistoPendente | LoginResponse>(`${this.baseUrl}/Player/create-profile`, dados, {
        headers: this.cabecalhos(turnstile),
      })
      .pipe(
        tap((resposta) => {
          if (resposta && 'firebaseLoginResponseDto' in resposta && resposta.firebaseLoginResponseDto) {
            this.aceitarSessao(resposta);
          }
        })
      );
  }

  private aceitarSessao(response: LoginResponse): void {
    this.guardar({
      jogadorId: response.firebaseLoginResponseDto.localId,
      equipaId: response.idTeam ?? null,
      admin: response.idTeam != null && response.isAdmin === true,
    });
  }

  /** Volta a enviar a confirmação do e-mail. A resposta é sempre a mesma. */
  reenviarConfirmacao(email: string, password: string, turnstile?: string | null): Observable<void> {
    return this.http
      .post(`${this.baseUrl}/User/resend-verification`, { email, password, website: '' }, { headers: this.cabecalhos(turnstile) })
      .pipe(map(() => undefined));
  }

  /** Pede a mensagem de recuperação da palavra-passe. A resposta é sempre a mesma. */
  recuperarPalavraPasse(email: string, turnstile?: string | null, website = ''): Observable<void> {
    return this.http
      .post(`${this.baseUrl}/User/forgot-password`, { email, website }, { headers: this.cabecalhos(turnstile) })
      .pipe(map(() => undefined));
  }

  /**
   * Pede à API um ID token novo (o refresh token viaja no cookie). Pedidos simultâneos partilham
   * a mesma renovação.
   * @returns `true` se a sessão foi renovada
   */
  renovarSessao(): Observable<boolean> {
    if (!this.renovacao$) {
      this.renovacao$ = this.http.post<void>(`${this.baseUrl}/User/refresh`, null).pipe(
        map(() => true),
        catchError(() => of(false)),
        finalize(() => (this.renovacao$ = null)),
        shareReplay(1)
      );
    }
    return this.renovacao$;
  }

  /**
   * Termina a sessão no servidor (revoga os tokens do Firebase e apaga os cookies) e apaga os
   * dados do browser. A sessão local é apagada mesmo que o pedido ao servidor falhe.
   */
  logout(): Observable<void> {
    if (!this.isAuthenticated()) {
      this.clearSession();
      return of(undefined);
    }
    return this.http.post<void>(`${this.baseUrl}/User/logout`, null).pipe(
      catchError(() => of(undefined)),
      tap(() => this.clearSession()),
      map(() => undefined)
    );
  }

  /** Apaga os dados da sessão guardados no browser. */
  clearSession(): void {
    try {
      localStorage.removeItem(AuthService.CHAVE);
    } catch {
      // Sem armazenamento: não há nada a apagar.
    }
    this.atualizarSessao();
  }

  /** Atualiza a equipa e o papel do utilizador (depois de criar, sair ou apagar uma equipa). */
  setTeam(teamId: string | null, isAdmin: boolean): void {
    const atual = this.lerGuardada();
    if (!atual) {
      return;
    }
    this.guardar({ ...atual, equipaId: teamId, admin: teamId !== null && isAdmin });
  }

  /** Volta a ler o estado guardado. */
  atualizarSessao(): void {
    this._sessao.set(this.lerSessao());
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
    const guardado = this.lerGuardada()?.equipaId;
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

  private cabecalhos(turnstile?: string | null): HttpHeaders {
    return turnstile ? new HttpHeaders({ [CABECALHO_TURNSTILE]: turnstile }) : new HttpHeaders();
  }

  private lerSessao(): EstadoSessao {
    const s = this.lerGuardada();
    return {
      autenticado: s !== null,
      jogadorId: s?.jogadorId ?? null,
      equipaId: s?.equipaId ?? null,
      admin: !!s && s.admin && !!s.equipaId,
    };
  }

  private lerGuardada(): SessaoGuardada | null {
    try {
      const texto = localStorage.getItem(AuthService.CHAVE);
      if (!texto) {
        return null;
      }
      const s = JSON.parse(texto) as Partial<SessaoGuardada>;
      return typeof s.jogadorId === 'string' && s.jogadorId
        ? { jogadorId: s.jogadorId, equipaId: s.equipaId ?? null, admin: s.admin === true }
        : null;
    } catch {
      return null;
    }
  }

  private guardar(s: SessaoGuardada): void {
    try {
      localStorage.setItem(AuthService.CHAVE, JSON.stringify(s));
    } catch {
      // Sem armazenamento (modo privado restrito): a sessão só dura até ao refresh.
    }
    this.atualizarSessao();
  }
}
