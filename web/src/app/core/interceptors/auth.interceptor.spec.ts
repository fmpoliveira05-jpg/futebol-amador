import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router, provideRouter } from '@angular/router';
import { CookieService } from 'ngx-cookie-service';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../environments/environment';
import { tokenQueExpiraEm } from '../../testing/token';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let http: HttpTestingController;
  let cookies: CookieService;
  let router: Router;
  const api = environment.apiBaseUrl;
  const token = tokenQueExpiraEm(3600);

  beforeEach(() => {
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
    cookies = TestBed.inject(CookieService);
    router = TestBed.inject(Router);
    cookies.set(AuthService.TOKEN, token, { path: '/' });
  });

  afterEach(() => {
    http.verify();
    TestBed.inject(AuthService).clearSession();
  });

  it('junta o token aos pedidos à API', () => {
    httpClient.get(`${api}/Team/1`).subscribe();
    expect(http.expectOne(`${api}/Team/1`).request.headers.get('Authorization')).toBe(`Bearer ${token}`);
  });

  it('não envia o token para outros domínios', () => {
    httpClient.get('https://outro-site.pt/dados').subscribe();
    expect(http.expectOne('https://outro-site.pt/dados').request.headers.has('Authorization')).toBeFalse();
  });

  it('termina a sessão e vai para o login quando a API responde 401', () => {
    const navegar = spyOn(router, 'navigate').and.resolveTo(true);
    let recebeuErro = false;
    httpClient.get(`${api}/Team/1`).subscribe({ error: () => (recebeuErro = true) });
    http.expectOne(`${api}/Team/1`).flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(recebeuErro).toBeTrue();
    expect(cookies.get(AuthService.TOKEN)).toBe('');
    expect(navegar).toHaveBeenCalledWith(['/login'], { queryParams: { sessao: 'expirada' } });
  });

  it('mantém a sessão noutros erros', () => {
    httpClient.get(`${api}/Team/1`).subscribe({ error: () => undefined });
    http.expectOne(`${api}/Team/1`).flush(null, { status: 403, statusText: 'Forbidden' });
    expect(cookies.get(AuthService.TOKEN)).toBe(token);
  });
});
