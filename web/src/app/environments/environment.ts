/**
 * Configuração de produção (`ng build`).
 * O endereço da API é o do servidor onde o backend estiver publicado.
 */
export const environment = {
  production: true,
  /** Modo demonstração: responde com dados de exemplo, sem backend. */
  demo: false,
  apiBaseUrl: 'https://amfootballapi.duckdns.org/api',
  /**
   * Chave pública (site key) do Cloudflare Turnstile. Vazia: o widget não aparece e a API não o
   * exige (a chave secreta, Turnstile:SecretKey, também fica vazia).
   */
  turnstileSiteKey: '',
  /**
   * Contacto para pedidos sobre dados pessoais (Política de Privacidade). Pôr o endereço real do
   * responsável antes de publicar.
   */
  contactoPrivacidade: 'privacidade@futebol-amador.example',
};
