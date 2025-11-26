import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

/**
 * Código de inicialização da aplicação Angular.
 * Realiza o "bootstrapping" da aplicação, ou seja, inicializa a aplicação Angular 
 * com a configuração fornecida e o componente principal (`App`).
 */

// Inicializa a aplicação Angular com o componente `App` e a configuração definida em `appConfig`
bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err)); // Captura e exibe qualquer erro durante a inicialização