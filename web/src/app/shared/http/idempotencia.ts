import { HttpContext } from '@angular/common/http';
import { CHAVE_IDEMPOTENCIA, novaChaveIdempotencia } from '../../core/interceptors/pedidos.interceptor';

/**
 * Contexto HTTP com uma `Idempotency-Key`. Um formulário guarda a mesma chave enquanto a operação
 * não tiver sucesso: se o utilizador repetir depois de um erro de rede, a API reconhece o pedido e
 * não cria nada em duplicado.
 */
export function comIdempotencia(chave: string = novaChaveIdempotencia()): { context: HttpContext } {
  return { context: new HttpContext().set(CHAVE_IDEMPOTENCIA, chave) };
}

export { novaChaveIdempotencia };
