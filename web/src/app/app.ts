import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SidebarComponent } from './components/partials/nav/sidebar/sidebar.component';

/** Componente raiz: menu lateral e a página da rota atual. */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SidebarComponent],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
