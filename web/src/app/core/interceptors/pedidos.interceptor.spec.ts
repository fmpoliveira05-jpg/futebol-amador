import { TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { HttpClient, HttpErrorResponse, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TEMPOS_PEDIDOS, pedidosInterceptor } from './pedidos.interceptor';
import { environment } from '../../environments/environment';
import { comIdempotencia } from '../../shared/http/idempotencia';
import { mensagemDeErro } from '../../shared/http/erros';

const esperar = (ms: number) => new Promise((r) => setTimeout(r, ms));

describe('pedidosInterceptor', () => {
  let httpClient: HttpClient;
  let http: HttpTestingController;
  const api = environment.apiBaseUrl;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideZonelessChangeDetection(),
        provideHttpClient(withInterceptors([pedidosInterceptor])),
        provideHttpClientTesting(),
        // Tempos curtos: a app corre sem zone.js (sem fakeAsync), por isso espera-se a sério.
        { provide: TEMPOS_PEDIDOS, useValue: { maximoMs: 60, esperaNovaTentativaMs: 10 } },
      ],
    });
    httpClient = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    http.verify();
  });

  it('repete uma vez um GET que falhou por rede', async () => {
    let resposta: unknown;
    httpClient.get(`${api}/leagues`).subscribe((r) => (resposta = r));

    http.expectOne(`${api}/leagues`).error(new ProgressEvent('error'), { status: 0 });
    await esperar(30);
    http.expectOne(`${api}/leagues`).flush([{ id: 'l1' }]);

    expect(resposta).toEqual([{ id: 'l1' }]);
  });

  it('não repete um GET com erro da API (4xx/5xx)', () => {
    let erro: HttpErrorResponse | undefined;
    httpClient.get(`${api}/leagues`).subscribe({ error: (e) => (erro = e) });

    http.expectOne(`${api}/leagues`).flush({ detail: 'x' }, { status: 500, statusText: 'Erro' });

    expect(erro?.status).toBe(500);
  });

  it('nunca repete um POST, mesmo com falha de rede', async () => {
    let erro: HttpErrorResponse | undefined;
    httpClient.post(`${api}/Team`, {}).subscribe({ error: (e) => (erro = e) });

    http.expectOne(`${api}/Team`).error(new ProgressEvent('error'), { status: 0 });
    await esperar(40);

    http.expectNone(`${api}/Team`);
    expect(erro?.status).toBe(0);
  });

  it('corta o pedido ao fim do tempo máximo (15 s) com uma mensagem própria', async () => {
    let erro: unknown;
    httpClient.post(`${api}/Team`, {}).subscribe({ error: (e) => (erro = e) });

    const pedido = http.expectOne(`${api}/Team`);
    await esperar(120);

    expect(pedido.cancelled).toBeTrue();
    expect(mensagemDeErro(erro)).toContain('demorou demasiado');
  });

  it('envia a Idempotency-Key quando o pedido a traz no contexto', () => {
    httpClient.post(`${api}/Team`, {}, comIdempotencia('chave-teste-123')).subscribe();

    const pedido = http.expectOne(`${api}/Team`);
    expect(pedido.request.headers.get('Idempotency-Key')).toBe('chave-teste-123');
    pedido.flush({});
  });

  it('não mexe em pedidos a outros domínios', () => {
    httpClient.post('https://api.cloudinary.com/v1_1/x/image/upload', {}, comIdempotencia('k-12345678')).subscribe();

    expect(http.expectOne('https://api.cloudinary.com/v1_1/x/image/upload').request.headers.has('Idempotency-Key')).toBeFalse();
  });
});
