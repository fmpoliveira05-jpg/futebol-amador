import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { demoInterceptor, reporDadosDemo } from './demo.interceptor';
import { DEMO_EQUIPA_ID } from './dados-demo';
import { environment } from '../../environments/environment';

describe('demoInterceptor', () => {
  let http: HttpClient;
  const api = environment.apiBaseUrl;

  beforeEach(() => {
    reporDadosDemo();
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideHttpClient(withInterceptors([demoInterceptor]))],
    });
    http = TestBed.inject(HttpClient);
  });

  it('faz login com qualquer credencial e devolve um token com exp', async () => {
    const r = await firstValueFrom(http.post<any>(`${api}/User/login`, { email: 'x@y.pt', password: '123456' }));
    const payload = JSON.parse(atob(r.firebaseLoginResponseDto.idToken.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')));
    expect(payload.exp).toBeGreaterThan(Date.now() / 1000);
    expect(r.idTeam).toBe(DEMO_EQUIPA_ID);
  });

  it('ordena a classificação por pontos', async () => {
    const lista = await firstValueFrom(http.get<any[]>(`${api}/Leaderboard`));
    expect(lista[0].position).toBe(1);
    expect(lista.map((t) => t.currentPoints)).toEqual([...lista.map((t) => t.currentPoints)].sort((a, b) => b - a));
  });

  it('aceitar um pedido de adesão põe o jogador na equipa', async () => {
    const antes = await firstValueFrom(http.get<any[]>(`${api}/Team/${DEMO_EQUIPA_ID}/membership-request`));
    await firstValueFrom(http.post(`${api}/Team/${DEMO_EQUIPA_ID}/membership-request/accept`, { requestId: antes[0].requestId }));
    const membros = await firstValueFrom(http.get<any[]>(`${api}/Team/${DEMO_EQUIPA_ID}/members`));
    expect(membros.some((m) => m.playerId === antes[0].player.id)).toBeTrue();
  });

  it('aplica os filtros de pesquisa de equipas', async () => {
    const lista = await firstValueFrom(http.get<any[]>(`${api}/Team/${DEMO_EQUIPA_ID}/search`, { params: { NameRank: 'Divisão 1' } }));
    expect(lista.length).toBeGreaterThan(0);
    expect(lista.every((t) => t.rank.name === 'Divisão 1')).toBeTrue();
  });

  it('responde 404 a rotas sem dados', async () => {
    await expectAsync(firstValueFrom(http.get(`${api}/Nada/aqui`))).toBeRejectedWith(jasmine.any(HttpErrorResponse));
  });
});
