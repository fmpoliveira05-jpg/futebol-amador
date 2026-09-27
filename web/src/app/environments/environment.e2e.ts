/**
 * Testes de ponta a ponta contra a build de produção (sem o modo demonstração): a API é simulada
 * pelo Playwright (`page.route`) no mesmo endereço, em `/api`. Ver e2e/armazenamento.spec.ts.
 */
export const environment = {
  production: true,
  demo: false,
  apiBaseUrl: '/api',
  turnstileSiteKey: '',
  contactoPrivacidade: 'privacidade@futebol-amador.example',
};
