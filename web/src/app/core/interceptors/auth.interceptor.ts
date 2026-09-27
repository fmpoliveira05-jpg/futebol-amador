import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, switchMap, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from '../../services/auth.service';

/** Cabeçalho exigido pela API nos pedidos com a sessão em cookie (proteção CSRF). */
export const CABECALHO_CSRF = 'X-Requested-With';
export const VALOR_CSRF = 'FutebolAmador';

/** Pedidos em que um 401 não significa "sessão expirada". */
const SEM_RENOVACAO = /\/User\/(login|refresh|logout|resend-verification|forgot-password)$/;

/**
 * Pedidos à API (e só a esses): envia os cookies da sessão (`withCredentials`) e o cabeçalho
 * anti-CSRF. Outros domínios nunca recebem cookies nem cabeçalhos.
 *
 * Se a API responder 401, o ID token (1 hora) expirou ou foi revogado: pede-se uma renovação
 * (`POST /User/refresh`) e repete-se o pedido uma vez. Se a renovação falhar, apaga-se a sessão
 * e o utilizador volta ao login.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const paraApi = req.url.startsWith(environment.apiBaseUrl);
  if (!paraApi) {
    return next(req);
  }

  const auth = inject(AuthService);
  const router = inject(Router);
  const pedido = req.clone({ withCredentials: true, setHeaders: { [CABECALHO_CSRF]: VALOR_CSRF } });

  const terminarSessao = () => {
    auth.clearSession();
    router.navigate(['/login'], { queryParams: { sessao: 'expirada' } });
  };

  return next(pedido).pipe(
    catchError((erro: unknown) => {
      if (!(erro instanceof HttpErrorResponse) || erro.status !== 401 || SEM_RENOVACAO.test(req.url) || !auth.isAuthenticated()) {
        return throwError(() => erro);
      }
      return auth.renovarSessao().pipe(
        switchMap((renovada) => {
          if (!renovada) {
            terminarSessao();
            return throwError(() => erro);
          }
          return next(pedido).pipe(
            catchError((erroRepetido: unknown) => {
              if (erroRepetido instanceof HttpErrorResponse && erroRepetido.status === 401) {
                terminarSessao();
              }
              return throwError(() => erroRepetido);
            })
          );
        })
      );
    })
  );
};
