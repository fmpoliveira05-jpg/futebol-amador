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

  it('faz login com qualquer credencial e, como a API real, não põe tokens no corpo', async () => {
    const r = await firstValueFrom(http.post<any>(`${api}/User/login`, { email: 'x@y.pt', password: '123456' }));
    expect(r.firebaseLoginResponseDto.localId).toBeTruthy();
    expect(r.firebaseLoginResponseDto.idToken).toBeUndefined();
    expect(r.idTeam).toBe(DEMO_EQUIPA_ID);
  });

  it('calcula a classificação da liga (3/1/0) com a forma dos últimos 5 jogos', async () => {
    const t = await firstValueFrom(http.get<any>(`${api}/Leaderboard`));
    const pontos = t.rows.map((r: any) => r.points);
    expect(pontos).toEqual([...pontos].sort((a: number, b: number) => b - a));
    for (const r of t.rows) {
      expect(r.points).toBe(r.won * 3 + r.drawn);
      expect(r.played).toBe(r.won + r.drawn + r.lost);
      expect(r.form.length).toBeLessThanOrEqual(5);
    }
  });

  it('transferência: o clube aceita e o jogador muda de equipa', async () => {
    const recebidas = await firstValueFrom(http.get<any>(`${api}/transfers/offers/team/${DEMO_EQUIPA_ID}`));
    const proposta = recebidas.received.find((o: any) => o.status === 0);
    await firstValueFrom(http.post(`${api}/transfers/offers/${proposta.id}/accept`, {}));
    const perfil = await firstValueFrom(http.get<any>(`${api}/Player/${proposta.playerId}/profile`));
    expect(perfil.currentTeam.idTeam).toBe(proposta.toTeamId);
    expect(perfil.transfers[0].kind).toBe('TRANSFERENCIA');
  });

  it('o mercado filtra por nacionalidade e não mostra a própria equipa', async () => {
    const lista = await firstValueFrom(
      http.get<any[]>(`${api}/transfers/market/${DEMO_EQUIPA_ID}`, { params: { nationality: 'Brasil' } })
    );
    expect(lista.length).toBeGreaterThan(0);
    expect(lista.every((p) => p.nationality === 'Brasil' && p.teamId !== DEMO_EQUIPA_ID)).toBeTrue();
  });

  it('o perfil conta golos e minutos a partir dos onzes e dos eventos', async () => {
    const perfil = await firstValueFrom(http.get<any>(`${api}/Player/demo-kiko/profile`));
    expect(perfil.totals.goals).toBe(1);
    expect(perfil.totals.games).toBe(2);
    expect(perfil.totals.minutes).toBe(180);
  });

  it('não deixa cancelar um jogo da liga sem nova data', async () => {
    await expectAsync(
      firstValueFrom(
        http.delete(`${api}/Calendar/${DEMO_EQUIPA_ID}/CancelMatch/jogo-8`, { body: JSON.stringify('Chuva'), headers: { 'Content-Type': 'application/json' } })
      )
    ).toBeRejectedWith(jasmine.any(HttpErrorResponse));
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
