import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { CABECALHO_CSRF, VALOR_CSRF, authInterceptor } from './auth.interceptor';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../environments/environment';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let http: HttpTestingController;
  let router: Router;
  let auth: AuthService;
  const api = environment.apiBaseUrl;

  beforeEach(() => {
    localStorage.setItem(AuthService.CHAVE, JSON.stringify({ jogadorId: 'jogador-1', equipaId: null, admin: false }));
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
      ],
    });
    httpClient = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
    auth = TestBed.inject(AuthService);
  });

  afterEach(() => {
    http.verify();
    auth.clearSession();
  });

  it('envia os cookies e o cabeçalho anti-CSRF à API, sem Authorization', () => {
    httpClient.post(`${api}/Team/1`, {}).subscribe();
    const pedido = http.expectOne(`${api}/Team/1`).request;
    expect(pedido.withCredentials).toBeTrue();
    expect(pedido.headers.get(CABECALHO_CSRF)).toBe(VALOR_CSRF);
    expect(pedido.headers.has('Authorization')).toBeFalse();
  });

  it('não envia cookies nem cabeçalhos para outros domínios', () => {
    httpClient.get('https://outro-site.pt/dados').subscribe();
    const pedido = http.expectOne('https://outro-site.pt/dados').request;
    expect(pedido.withCredentials).toBeFalse();
    expect(pedido.headers.has(CABECALHO_CSRF)).toBeFalse();
  });

  it('num 401 renova a sessão uma vez e repete o pedido', () => {
    let resposta: unknown;
    httpClient.get(`${api}/Team/1`).subscribe((r) => (resposta = r));
    http.expectOne(`${api}/Team/1`).flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne(`${api}/User/refresh`).flush(null, { status: 204, statusText: 'No Content' });
    http.expectOne(`${api}/Team/1`).flush({ ok: true });

    expect(resposta).toEqual({ ok: true });
    expect(auth.isAuthenticated()).toBeTrue();
  });

  it('se a renovação falhar, termina a sessão e vai para o login', () => {
    const navegar = spyOn(router, 'navigate').and.resolveTo(true);
    let recebeuErro = false;
    httpClient.get(`${api}/Team/1`).subscribe({ error: () => (recebeuErro = true) });
    http.expectOne(`${api}/Team/1`).flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectOne(`${api}/User/refresh`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(recebeuErro).toBeTrue();
    expect(auth.isAuthenticated()).toBeFalse();
    expect(navegar).toHaveBeenCalledWith(['/login'], { queryParams: { sessao: 'expirada' } });
  });

  it('um 401 no login não tenta renovar a sessão', () => {
    httpClient.post(`${api}/User/login`, {}).subscribe({ error: () => undefined });
    http.expectOne(`${api}/User/login`).flush(null, { status: 401, statusText: 'Unauthorized' });
    http.expectNone(`${api}/User/refresh`);
  });

  it('mantém a sessão noutros erros', () => {
    httpClient.get(`${api}/Team/1`).subscribe({ error: () => undefined });
    http.expectOne(`${api}/Team/1`).flush(null, { status: 403, statusText: 'Forbidden' });
    expect(auth.isAuthenticated()).toBeTrue();
  });
});
