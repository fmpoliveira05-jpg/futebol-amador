import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';

/** Termina a sessão e volta ao login. */
@Component({
  selector: 'app-logout',
  template: '<p class="a-sair">A terminar a sessão…</p>',
})
export class LogoutComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/login']));
  }
}
