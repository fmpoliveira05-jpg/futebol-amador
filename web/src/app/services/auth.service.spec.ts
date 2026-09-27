import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService, CABECALHO_TURNSTILE, LoginResponse } from './auth.service';
import { environment } from '../environments/environment';

describe('AuthService', () => {
  let auth: AuthService;
  let http: HttpTestingController;
  const api = environment.apiBaseUrl;

  const respostaLogin = (idTeam: string | null, isAdmin: boolean): LoginResponse => ({
    name: 'Francisco',
    idTeam,
    isAdmin,
    firebaseLoginResponseDto: { localId: 'jogador-1', expiresIn: '3600' },
  });

  const guardar = (valor: object) => localStorage.setItem(AuthService.CHAVE, JSON.stringify(valor));

  beforeEach(() => {
    localStorage.removeItem(AuthService.CHAVE);
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(), provideHttpClientTesting()],
    });
    auth = TestBed.inject(AuthService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
    auth.clearSession();
  });

  it('guarda só indicações da sessão depois do login (nunca tokens)', () => {
    auth.login('a@b.pt', 'segredo', 'token-turnstile').subscribe();
    const pedido = http.expectOne(`${api}/User/login`);
    expect(pedido.request.body).toEqual({ email: 'a@b.pt', password: 'segredo', website: '' });
    expect(pedido.request.headers.get(CABECALHO_TURNSTILE)).toBe('token-turnstile');
    pedido.flush(respostaLogin('equipa-1', true));

    expect(auth.getCurrentPlayerId()).toBe('jogador-1');
    expect(auth.isAuthenticated()).toBeTrue();
    expect(auth.isAdmin()).toBeTrue();
    expect(auth.hasTeam()).toBeTrue();
    expect(auth.sessao()).toEqual({ autenticado: true, jogadorId: 'jogador-1', equipaId: 'equipa-1', admin: true });

    const guardado = localStorage.getItem(AuthService.CHAVE) ?? '';
    expect(guardado).not.toContain('token');
    expect(document.cookie).not.toContain('access_token');
  });

  it('um jogador sem equipa nunca fica como administrador', () => {
    auth.login('a@b.pt', 'segredo').subscribe();
    http.expectOne(`${api}/User/login`).flush(respostaLogin(null, true));
    expect(auth.hasTeam()).toBeFalse();
    expect(auth.isAdmin()).toBeFalse();
  });

  it('ignora dados guardados inválidos', () => {
    localStorage.setItem(AuthService.CHAVE, 'isto não é json');
    expect(auth.isAuthenticated()).toBeFalse();
    guardar({ equipaId: 'x' });
    expect(auth.isAuthenticated()).toBeFalse();
  });

  it('termina a sessão com POST e apaga-a mesmo que o servidor falhe', () => {
    guardar({ jogadorId: 'jogador-1', equipaId: null, admin: false });
    let terminou = false;
    auth.logout().subscribe(() => (terminou = true));
    const pedido = http.expectOne(`${api}/User/logout`);
    expect(pedido.request.method).toBe('POST');
    pedido.flush(null, { status: 500, statusText: 'Erro' });

    expect(terminou).toBeTrue();
    expect(auth.getCurrentPlayerId()).toBeNull();
    expect(auth.sessao().autenticado).toBeFalse();
  });

  it('partilha uma única renovação entre pedidos simultâneos', () => {
    const resultados: boolean[] = [];
    auth.renovarSessao().subscribe((r) => resultados.push(r));
    auth.renovarSessao().subscribe((r) => resultados.push(r));
    http.expectOne(`${api}/User/refresh`).flush(null, { status: 204, statusText: 'No Content' });
    expect(resultados).toEqual([true, true]);

    auth.renovarSessao().subscribe((r) => resultados.push(r));
    http.expectOne(`${api}/User/refresh`).flush(null, { status: 401, statusText: 'Unauthorized' });
    expect(resultados).toEqual([true, true, false]);
  });

  it('no registo com confirmação pendente não fica autenticado', () => {
    auth
      .signup({ name: 'X', email: 'a@b.pt', password: 'Futebol#2026', dateOfBirth: '2000-01-01', address: 'Rua, Braga', phone: '+351912345678', position: 0, height: 180 })
      .subscribe();
    http.expectOne(`${api}/Player/create-profile`).flush({ playerId: 'p', verificacaoEmailPendente: true, mensagem: 'ok' });
    expect(auth.isAuthenticated()).toBeFalse();
  });

  it('atualiza a equipa guardada ao ler os dados do próprio jogador', () => {
    guardar({ jogadorId: 'jogador-1', equipaId: null, admin: false });
    auth.getPlayerData('jogador-1').subscribe();
    http.expectOne(`${api}/Player/details/jogador-1`).flush({ playerId: 'jogador-1', team: { idTeam: 'nova', name: 'X' }, isAdmin: false });

    expect(auth.sessao().equipaId).toBe('nova');
    expect(auth.isAdmin()).toBeFalse();
  });

  it('não mexe na sessão ao ler os dados de outro jogador', () => {
    guardar({ jogadorId: 'jogador-1', equipaId: null, admin: false });
    auth.getPlayerData('outro').subscribe();
    http.expectOne(`${api}/Player/details/outro`).flush({ playerId: 'outro', team: { idTeam: 'deles', name: 'Y' }, isAdmin: true });
    expect(auth.hasTeam()).toBeFalse();
  });

  it('usa a equipa guardada sem ir à API', () => {
    guardar({ jogadorId: 'jogador-1', equipaId: 'equipa-1', admin: false });
    let equipa: string | null = null;
    auth.getCurrentTeamId().subscribe((id) => (equipa = id));
    expect(equipa as string | null).toBe('equipa-1');
  });
});
