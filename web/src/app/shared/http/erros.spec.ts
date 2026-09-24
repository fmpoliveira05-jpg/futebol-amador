import { HttpErrorResponse } from '@angular/common/http';
import { mensagemDeErro } from './erros';

describe('mensagemDeErro', () => {
  const http = (status: number, error: unknown) => new HttpErrorResponse({ status, error });

  it('junta os erros de validação do ASP.NET', () => {
    const erro = http(400, { errors: { Name: ['Nome obrigatório.'], Height: ['Altura inválida.'] } });
    expect(mensagemDeErro(erro)).toBe('Nome obrigatório. Altura inválida.');
  });

  it('usa o detail de um ProblemDetails', () => {
    expect(mensagemDeErro(http(409, { title: 'Conflito', detail: 'A equipa já existe.' }))).toBe('A equipa já existe.');
  });

  it('aceita texto simples', () => {
    expect(mensagemDeErro(http(400, 'Pedido inválido.'))).toBe('Pedido inválido.');
  });

  it('explica a falta de ligação', () => {
    expect(mensagemDeErro(http(0, null))).toContain('servidor');
  });

  it('tem mensagens para 403 e 404 sem corpo', () => {
    expect(mensagemDeErro(http(403, null))).toContain('permissão');
    expect(mensagemDeErro(http(404, null))).toBe('Não encontrado.');
  });

  it('usa a mensagem predefinida nos restantes casos', () => {
    expect(mensagemDeErro(http(500, null), 'Falhou.')).toBe('Falhou.');
    expect(mensagemDeErro('qualquer coisa', 'Falhou.')).toBe('Falhou.');
  });

  it('usa a mensagem de um Error normal', () => {
    expect(mensagemDeErro(new Error('O utilizador não tem equipa.'))).toBe('O utilizador não tem equipa.');
  });
});
