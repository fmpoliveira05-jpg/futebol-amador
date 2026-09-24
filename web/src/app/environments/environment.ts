/**
 * Configuração de produção (`ng build`).
 * O endereço da API é o do servidor onde o backend estiver publicado.
 */
export const environment = {
  production: true,
  /** Modo demonstração: responde com dados de exemplo, sem backend. */
  demo: false,
  apiBaseUrl: 'https://amfootballapi.duckdns.org/api',
};
