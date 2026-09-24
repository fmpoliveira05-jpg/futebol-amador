/**
 * Ambiente de configuração para o desenvolvimento.
 * Define variáveis específicas para o ambiente de desenvolvimento, como o estado de produção e a URL da API.
 */
export const environment = {
  
  /**
   * Define se o ambiente é de produção ou desenvolvimento.
   * Neste caso, está definido como `false`, indicando que é o ambiente de desenvolvimento.
   */
  production: false,

  /**
   * A URL base para a API do backend.
   * A URL foi configurada para um servidor local durante o desenvolvimento.
   */
  apiBaseUrl: 'http://localhost:5218/api',
};