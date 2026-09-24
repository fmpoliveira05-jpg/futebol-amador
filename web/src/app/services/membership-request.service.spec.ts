import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { MembershipRequestService } from './membership-request.service';
import { AuthService } from './auth.service';
import { environment } from '../environments/environment';

describe('MembershipRequestService', () => {
  let service: MembershipRequestService;
  let http: HttpTestingController;
  let auth: jasmine.SpyObj<AuthService>;
  const api = environment.apiBaseUrl;

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', ['getCurrentPlayerId', 'getCurrentTeamId']);
    auth.getCurrentPlayerId.and.returnValue('jogador-1');
    auth.getCurrentTeamId.and.returnValue(of('equipa-1'));
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: auth },
      ],
    });
    service = TestBed.inject(MembershipRequestService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  // A API recebe objetos ({ requestId }, { teamId }, { playerId }) e não strings soltas.
  it('aceita um convite com o id no corpo', () => {
    service.acceptMembershipRequestPlayer('pedido-9').subscribe();
    const req = http.expectOne(`${api}/Player/jogador-1/membership-requests/accept`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ requestId: 'pedido-9' });
  });

  it('pede para entrar numa equipa', () => {
    service.sendMembershipRequestPlayer('equipa-7').subscribe();
    expect(http.expectOne(`${api}/Player/jogador-1/membership-requests/send`).request.body).toEqual({ teamId: 'equipa-7' });
  });

  it('convida um jogador em nome da equipa', () => {
    service.sendMembershipRequestTeam('jogador-5').subscribe();
    expect(http.expectOne(`${api}/Team/equipa-1/membership-requests/send`).request.body).toEqual({ playerId: 'jogador-5' });
  });

  it('falha sem pedido HTTP quando o utilizador não tem equipa', () => {
    auth.getCurrentTeamId.and.returnValue(of(null));
    let erro: unknown;
    service.sendMembershipRequestTeam('jogador-5').subscribe({ error: (e) => (erro = e) });
    expect(erro).toEqual(jasmine.any(Error));
  });
});
