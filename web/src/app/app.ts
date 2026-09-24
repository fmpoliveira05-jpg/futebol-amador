import { Component, inject, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { TeamService } from './services/team.service';
import { SidebarComponent } from './components/partials/nav/sidebar/sidebar.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localePt from '@angular/common/locales/pt';

registerLocaleData(localePt);

/**
 * Componente principal da aplicação.
 * Contém o layout básico, incluindo o componente de navegação lateral e a lógica para a renderização das páginas.
 */
@Component({
  selector: 'app-root', // Seletor para o componente raiz da aplicação
  imports: [RouterOutlet, SidebarComponent, RouterModule], // Importação dos módulos e componentes necessários
  templateUrl: './app.html', // Template HTML associado ao componente
  styleUrl: './app.css', // Estilo CSS associado ao componente
  standalone: true, // Indica que o componente é stand-alone, ou seja, não depende de módulos externos
  providers: [
    /**
     * Fornece o interceptador HTTP personalizado para adicionar o token de autenticação às requisições.
     * O `multi: true` permite que múltiplos interceptadores sejam usados ao mesmo tempo.
     */
    { provide: HTTP_INTERCEPTORS, useValue: authInterceptor, multi: true },
    { provide: LOCALE_ID, useValue: 'pt-PT' }
  ]
})
export class App {

  /**
   * Variável de título, usando `signal` para que seja reativa.
   * O título é utilizado em algumas partes da interface da aplicação.
   */
  protected readonly title = signal('frontend');

  /**
   * Lista de equipas carregadas através do serviço de equipas.
   * Inicialmente é um array vazio, mas será preenchido com os dados das equipas.
   */
  teams: any[] = [];

  /**
   * Serviço para comunicação com a API de equipas.
   * Utilizado para obter dados e realizar ações relacionadas às equipas.
   */
  teamService = inject(TeamService);

  /**
   * Construtor do componente. Neste caso, o construtor está vazio,
   * mas o `teamService` é injetado para ser utilizado dentro do componente.
   */
  constructor() {}
}