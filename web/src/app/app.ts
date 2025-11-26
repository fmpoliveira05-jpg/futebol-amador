import { Component, inject, signal } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { TeamService } from './services/team.service';
import { SidebarComponent } from './components/partials/nav/sidebar/sidebar.component';
import { HTTP_INTERCEPTORS } from '@angular/common/http';
import { authInterceptor } from './core/interceptors/auth.interceptor';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, SidebarComponent, RouterModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
  standalone: true,
  providers: [
    { provide: HTTP_INTERCEPTORS, useValue: authInterceptor, multi: true },
  ]
})
export class App {
  protected readonly title = signal('frontend');

  teams: any[] = [];
  teamService = inject(TeamService);

  constructor() {}
}
