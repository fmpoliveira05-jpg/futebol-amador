/**
 * Ambiente de configuração para a produção.
 * Define variáveis específicas para o ambiente de produção, como o estado de produção e a URL da API.
 */
export const environment = {
  
  /**
   * Define se o ambiente é de produção ou desenvolvimento.
   * Neste caso, está definido como `true`, indicando que é o ambiente de produção.
   */
  production: true,

  /**
   * A URL base para a API do backend.
   * A URL foi configurada para um servidor local durante a produção.
   */
  apiBaseUrl: 'https://amfootballapi.duckdns.org/api',
}