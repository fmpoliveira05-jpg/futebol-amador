import {
  ApplicationConfig,
  LOCALE_ID,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter, withComponentInputBinding, withInMemoryScrolling } from '@angular/router';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt-PT';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { environment } from './environments/environment';
import { demoInterceptor } from './core/demo/demo.interceptor';

registerLocaleData(localePt, 'pt-PT');

/**
 * Configuração da aplicação: deteção de alterações sem zone.js (o estado dos componentes está
 * em signals), rotas e cliente HTTP com o interceptor de autenticação. No modo demonstração,
 * um segundo interceptor responde aos pedidos com dados de exemplo, sem backend.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(
      routes,
      withComponentInputBinding(),
      withInMemoryScrolling({ scrollPositionRestoration: 'top' })
    ),
    { provide: LOCALE_ID, useValue: 'pt-PT' },
    provideHttpClient(
      withInterceptors(environment.demo ? [authInterceptor, demoInterceptor] : [authInterceptor])
    ),
  ],
};
