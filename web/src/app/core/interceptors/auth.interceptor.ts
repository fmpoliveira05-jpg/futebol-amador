import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from '../../services/auth.service';

/**
 * Junta o token de acesso aos pedidos feitos à API (e só a esses: outros domínios nunca recebem
 * o token). Se a API responder 401, a sessão expirou ou foi revogada: apaga-se a sessão e o
 * utilizador volta ao login.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const paraApi = req.url.startsWith(environment.apiBaseUrl);
  const token = auth.getToken();

  const pedido = paraApi && token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(pedido).pipe(
    catchError((erro: unknown) => {
      if (paraApi && erro instanceof HttpErrorResponse && erro.status === 401 && token) {
        auth.clearSession();
        router.navigate(['/login'], { queryParams: { sessao: 'expirada' } });
      }
      return throwError(() => erro);
    })
  );
};
