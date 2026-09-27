import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { InjectionToken, inject } from '@angular/core';
import { TimeoutError, catchError, retry, throwError, timeout, timer } from 'rxjs';
import { environment } from '../../environments/environment';

/** Tempo máximo de um pedido à API (15 s): depois disso o utilizador vê uma mensagem. */
export const TEMPO_MAXIMO_MS = 15_000;

/** Espera antes da única nova tentativa de um GET que falhou por rede. */
export const ESPERA_NOVA_TENTATIVA_MS = 800;

/** Tempos do interceptor (injetáveis para os testes). */
export const TEMPOS_PEDIDOS = new InjectionToken<{ maximoMs: number; esperaNovaTentativaMs: number }>('TEMPOS_PEDIDOS', {
  factory: () => ({ maximoMs: TEMPO_MAXIMO_MS, esperaNovaTentativaMs: ESPERA_NOVA_TENTATIVA_MS }),
});

/** Chave de idempotência do pedido (cabeçalho `Idempotency-Key`), ver `comIdempotencia`. */
export const CHAVE_IDEMPOTENCIA = new HttpContextToken<string | null>(() => null);

/** Marca no corpo do erro para distinguir um timeout de uma falha de rede. */
export const ERRO_TIMEOUT = 'timeout';

/** Uma chave nova (UUID) para uma operação de criação. */
export function novaChaveIdempotencia(): string {
  if (typeof crypto !== 'undefined' && 'randomUUID' in crypto) {
    return crypto.randomUUID();
  }
  return `${Date.now().toString(36)}-${Math.random().toString(36).slice(2)}-${Math.random().toString(36).slice(2)}`;
}

/**
 * Pedidos à API:
 * - **tempo máximo** de 15 s — depois disso o pedido falha com uma mensagem própria
 *   (`mensagemDeErro` mostra "O servidor demorou demasiado a responder");
 * - **uma nova tentativa**, só para `GET` (idempotentes) e só quando a rede falha (estado 0 ou
 *   timeout) — nunca para POST/PUT/DELETE, que podiam ser aplicados duas vezes;
 * - **Idempotency-Key** nos pedidos que a trazem no contexto (`CHAVE_IDEMPOTENCIA`): se o mesmo
 *   pedido chegar duas vezes à API, a segunda devolve a resposta da primeira.
 */
export const pedidosInterceptor: HttpInterceptorFn = (req, next) => {
  if (!req.url.startsWith(environment.apiBaseUrl)) {
    return next(req);
  }

  const tempos = inject(TEMPOS_PEDIDOS);
  const chave = req.context.get(CHAVE_IDEMPOTENCIA);
  const pedido = chave ? req.clone({ setHeaders: { 'Idempotency-Key': chave } }) : req;

  const comTempo = next(pedido).pipe(
    // "each" e não "first": o HttpClient emite logo o evento "Sent"; o que conta é o tempo até à
    // resposta (e entre eventos de progresso, quando os há).
    timeout({ each: tempos.maximoMs }),
    catchError((erro: unknown) =>
      throwError(() =>
        erro instanceof TimeoutError
          ? new HttpErrorResponse({ status: 0, statusText: 'Timeout', url: req.url, error: { tipo: ERRO_TIMEOUT } })
          : erro
      )
    )
  );

  if (req.method !== 'GET') {
    return comTempo;
  }

  return comTempo.pipe(
    retry({
      count: 1,
      delay: (erro: unknown) =>
        erro instanceof HttpErrorResponse && erro.status === 0 ? timer(tempos.esperaNovaTentativaMs) : throwError(() => erro),
    })
  );
};
