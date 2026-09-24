/**
 * Modo demonstração (`npm run demo`): não precisa de backend. Os pedidos à API são respondidos
 * por `demo.interceptor.ts` com dados de exemplo guardados em memória.
 */
export const environment = {
  production: true,
  demo: true,
  apiBaseUrl: '/api',
};
