/**
 * Modo demonstração (`npm run demo`): não precisa de backend. Os pedidos à API são respondidos
 * por `demo.interceptor.ts` com dados de exemplo guardados em memória.
 */
export const environment = {
  production: true,
  demo: true,
  apiBaseUrl: '/api',
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
