import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../../services/auth.service';

/**
 * Protege as rotas da aplicação.
 *
 * - Sem sessão válida, envia para o login (e volta à página pedida depois de entrar).
 * - `data: { isAdmin: true }` exige ser administrador de uma equipa (confirmado na API).
 * - `data: { requiresTeam: true }` exige pertencer a uma equipa.
 * - `data: { requiresNoTeam: true }` exige não ter equipa (por exemplo, para criar uma).
 */
export const authGuard: CanActivateFn = (route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    auth.clearSession();
    return router.createUrlTree(['/login'], { queryParams: { voltar: state.url } });
  }

  if (route.data['requiresNoTeam'] && auth.hasTeam()) {
    return router.createUrlTree(['/players/me']);
  }

  if (route.data['requiresTeam'] && !auth.hasTeam()) {
    return router.createUrlTree(['/teams']);
  }

  if (route.data['isAdmin']) {
    return auth.canUserActivateAdminRoute().pipe(map((admin) => admin || router.createUrlTree(['/'])));
  }

  return true;
};
