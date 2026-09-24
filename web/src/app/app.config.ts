import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';
import {
  provideHttpClient,
  withInterceptors,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';

/**
 * Configuração principal da aplicação Angular.
 * Inclui todos os provedores necessários para a inicialização da aplicação, 
 * incluindo interceptadores, routing, tratamento de erros globais e mais.
 */
export const appConfig: ApplicationConfig = {
  providers: [
    /**
     * Provedor que adiciona ouvintes de erros globais do navegador.
     * Ajuda a capturar e lidar com erros globais que não são tratados em outros lugares.
     */
    provideBrowserGlobalErrorListeners(),

    /**
     * Provedor que configura a detecção de mudanças sem zona.
     * Permite que a detecção de mudanças no Angular ocorra sem o uso da zona de execução,
     * o que pode melhorar a performance em certos cenários.
     */
    provideZonelessChangeDetection(),

    /**
     * Provedor que configura o routing da aplicação, utilizando as rotas definidas em `app.routes`.
     * Fornece a configuração de navegação para a aplicação.
     */
    provideRouter(routes),

    /**
     * Provedor para a hidratação do cliente, com replay de eventos.
     * Hidratação é um processo usado para restaurar o estado da aplicação no cliente, 
     * e `withEventReplay` garante que os eventos de navegação sejam processados corretamente.
     */
    provideClientHydration(withEventReplay()),

    /**
     * Provedor do cliente HTTP, configurando interceptadores para as requisições HTTP.
     * O `withInterceptorsFromDi()` usa os interceptadores registados na DI, 
     * enquanto o `withInterceptors([authInterceptor])` adiciona um interceptador de autenticação.
     */
    provideHttpClient(
      withInterceptorsFromDi(),
      withInterceptors([authInterceptor]),
    ),
  ],
};