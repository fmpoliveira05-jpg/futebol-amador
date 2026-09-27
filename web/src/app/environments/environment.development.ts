/**
 * Configuração de desenvolvimento (`ng serve`): backend a correr localmente
 * (`dotnet run --project backend/Api`, porta definida em `launchSettings.json`).
 */
export const environment = {
  production: false,
  demo: false,
  apiBaseUrl: 'http://localhost:5218/api',
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
