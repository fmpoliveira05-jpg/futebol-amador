import { HttpErrorResponse } from '@angular/common/http';

/**
 * Transforma um erro da API numa mensagem para o utilizador.
 *
 * A API devolve `ProblemDetails` (`detail`), erros de validação do ASP.NET (`errors`, por campo)
 * ou, em alguns casos, texto simples. Sem ligação ao servidor o estado é 0.
 */
export function mensagemDeErro(erro: unknown, predefinida = 'Ocorreu um erro. Tenta novamente.'): string {
  if (!(erro instanceof HttpErrorResponse)) {
    return erro instanceof Error && erro.message ? erro.message : predefinida;
  }
  if (erro.status === 0) {
    return 'Não foi possível contactar o servidor. Verifica a ligação.';
  }
  const corpo = erro.error;
  if (corpo && typeof corpo === 'object') {
    if (corpo.errors && typeof corpo.errors === 'object') {
      const mensagens = Object.values(corpo.errors as Record<string, string[] | string>).flat();
      if (mensagens.length) {
        return mensagens.join(' ');
      }
    }
    if (typeof corpo.detail === 'string' && corpo.detail) {
      return corpo.detail;
    }
    if (typeof corpo.title === 'string' && corpo.title) {
      return corpo.title;
    }
  }
  if (typeof corpo === 'string' && corpo.trim() && corpo.length < 300) {
    return corpo;
  }
  if (erro.status === 403) {
    return 'Não tens permissão para fazer isto.';
  }
  if (erro.status === 404) {
    return 'Não encontrado.';
  }
  return predefinida;
}
