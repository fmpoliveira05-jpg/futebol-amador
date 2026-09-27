import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { SidebarComponent } from './components/partials/nav/sidebar/sidebar.component';

/** Componente raiz: menu lateral, a página da rota atual e o rodapé (Política de Privacidade). */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, SidebarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
