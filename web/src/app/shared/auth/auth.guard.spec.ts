import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot, UrlTree, provideRouter } from '@angular/router';
import { Observable, firstValueFrom, isObservable, of } from 'rxjs';
import { authGuard } from './auth.guard';
import { AuthService } from '../../services/auth.service';

describe('authGuard', () => {
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

  const correr = async (data: Record<string, unknown> = {}, url = '/pagina') => {
    const rota = { data } as unknown as ActivatedRouteSnapshot;
    const estado = { url } as RouterStateSnapshot;
    const r = TestBed.runInInjectionContext(() => authGuard(rota, estado));
    return isObservable(r) ? firstValueFrom(r as Observable<boolean | UrlTree>) : r;
  };
  const destino = (r: unknown) => (r instanceof UrlTree ? router.serializeUrl(r) : r);

  beforeEach(() => {
    auth = jasmine.createSpyObj<AuthService>('AuthService', [
      'isAuthenticated',
      'clearSession',
      'hasTeam',
      'canUserActivateAdminRoute',
    ]);
    TestBed.configureTestingModule({
      providers: [provideZonelessChangeDetection(), provideRouter([]), { provide: AuthService, useValue: auth }],
    });
    router = TestBed.inject(Router);
    auth.isAuthenticated.and.returnValue(true);
    auth.hasTeam.and.returnValue(false);
  });

  it('manda para o login quem não tem sessão, lembrando a página pedida', async () => {
    auth.isAuthenticated.and.returnValue(false);
    expect(destino(await correr({}, '/team/members'))).toBe('/login?voltar=%2Fteam%2Fmembers');
    expect(auth.clearSession).toHaveBeenCalled();
  });

  it('deixa passar um utilizador autenticado', async () => {
    expect(await correr()).toBeTrue();
  });

  it('não deixa criar equipa a quem já tem uma', async () => {
    auth.hasTeam.and.returnValue(true);
    expect(destino(await correr({ requiresNoTeam: true }))).toBe('/players/me');
  });

  it('exige equipa nas páginas da equipa', async () => {
    expect(destino(await correr({ requiresTeam: true }))).toBe('/teams');
  });

  it('confirma na API as páginas de administrador', async () => {
    auth.canUserActivateAdminRoute.and.returnValue(of(false));
    expect(destino(await correr({ isAdmin: true }))).toBe('/');

    auth.canUserActivateAdminRoute.and.returnValue(of(true));
    expect(await correr({ isAdmin: true })).toBeTrue();
  });
});
