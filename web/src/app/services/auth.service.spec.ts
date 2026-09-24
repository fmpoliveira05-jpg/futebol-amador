import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { CookieService } from 'ngx-cookie-service';
import { AuthService, LoginResponse } from './auth.service';
import { environment } from '../environments/environment';
import { tokenQueExpiraEm } from '../testing/token';

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;
  let cookies: CookieService;
  const api = environment.apiBaseUrl;

  const respostaLogin = (token: string, idTeam: string | null, isAdmin: boolean): LoginResponse => ({
    name: 'Francisco',
    idTeam,
    isAdmin,
    firebaseLoginResponseDto: { idToken: token, localId: 'jogador-1', expiresIn: '3600' },
  });

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
    cookies = TestBed.inject(CookieService);
    auth.clearSession();
  });

  afterEach(() => {
    http.verify();
    auth.clearSession();
  });

  it('guarda a sessão depois do login', () => {
    const token = tokenQueExpiraEm(3600);
    auth.login('a@b.pt', 'segredo').subscribe();
    const pedido = http.expectOne(`${api}/User/login`);
    expect(pedido.request.body).toEqual({ email: 'a@b.pt', password: 'segredo' });
    pedido.flush(respostaLogin(token, 'equipa-1', true));

    expect(auth.getToken()).toBe(token);
    expect(auth.getCurrentPlayerId()).toBe('jogador-1');
    expect(auth.isAuthenticated()).toBeTrue();
    expect(auth.isAdmin()).toBeTrue();
    expect(auth.hasTeam()).toBeTrue();
    expect(auth.sessao()).toEqual({ autenticado: true, jogadorId: 'jogador-1', equipaId: 'equipa-1', admin: true });
  });

  it('um jogador sem equipa nunca fica como administrador', () => {
    auth.login('a@b.pt', 'segredo').subscribe();
    http.expectOne(`${api}/User/login`).flush(respostaLogin(tokenQueExpiraEm(3600), null, true));
    expect(auth.hasTeam()).toBeFalse();
    expect(auth.isAdmin()).toBeFalse();
  });

  it('considera expirado um token cujo exp já passou', () => {
    cookies.set(AuthService.TOKEN, tokenQueExpiraEm(-60), { path: '/' });
    expect(auth.isAuthenticated()).toBeFalse();
  });

  it('considera expirado um token que expira dentro da margem de segurança', () => {
    cookies.set(AuthService.TOKEN, tokenQueExpiraEm(10), { path: '/' });
    expect(auth.isAuthenticated()).toBeFalse();
  });

  it('rejeita tokens mal formados', () => {
    cookies.set(AuthService.TOKEN, 'isto-nao-e-um-jwt', { path: '/' });
    expect(auth.isAuthenticated()).toBeFalse();
  });

  it('apaga a sessão no logout mesmo que o servidor falhe', () => {
    cookies.set(AuthService.TOKEN, tokenQueExpiraEm(3600), { path: '/' });
    cookies.set(AuthService.USER, 'jogador-1', { path: '/' });
    let terminou = false;
    auth.logout().subscribe(() => (terminou = true));
    http.expectOne(`${api}/User/logout`).flush(null, { status: 500, statusText: 'Erro' });

    expect(terminou).toBeTrue();
    expect(auth.getToken()).toBeNull();
    expect(auth.getCurrentPlayerId()).toBeNull();
    expect(auth.sessao().autenticado).toBeFalse();
  });

  it('atualiza a equipa guardada ao ler os dados do próprio jogador', () => {
    cookies.set(AuthService.TOKEN, tokenQueExpiraEm(3600), { path: '/' });
    cookies.set(AuthService.USER, 'jogador-1', { path: '/' });
    auth.getPlayerData('jogador-1').subscribe();
    http.expectOne(`${api}/Player/details/jogador-1`).flush({ playerId: 'jogador-1', team: { idTeam: 'nova', name: 'X' }, isAdmin: false });

    expect(cookies.get(AuthService.TEAM)).toBe('nova');
    expect(auth.isAdmin()).toBeFalse();
  });

  it('não mexe na sessão ao ler os dados de outro jogador', () => {
    cookies.set(AuthService.USER, 'jogador-1', { path: '/' });
    auth.getPlayerData('outro').subscribe();
    http.expectOne(`${api}/Player/details/outro`).flush({ playerId: 'outro', team: { idTeam: 'deles', name: 'Y' }, isAdmin: true });
    expect(cookies.get(AuthService.TEAM)).toBe('');
  });

  it('usa a equipa guardada sem ir à API', () => {
    cookies.set(AuthService.USER, 'jogador-1', { path: '/' });
    cookies.set(AuthService.TEAM, 'equipa-1', { path: '/' });
    let equipa: string | null = null;
    auth.getCurrentTeamId().subscribe((id) => (equipa = id));
    expect(equipa as string | null).toBe('equipa-1');
  });
});
