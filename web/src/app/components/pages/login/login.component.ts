import { Component, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { mensagemDeErro } from '../../../shared/http/erros';
import { environment } from '../../../environments/environment';

/** Página de login. Depois de entrar volta à página pedida (`?voltar=`) ou ao perfil. */
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [ReactiveFormsModule, RouterLink],
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  /** Parâmetros de query (ligados pelo router). */
  readonly voltar = input<string>();
  readonly sessao = input<string>();

  /** No modo demonstração qualquer e-mail e palavra-passe servem. */
  protected readonly demo = environment.demo;

  readonly aEntrar = signal(false);
  readonly erro = signal<string | null>(null);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    const { email, password } = this.loginForm.getRawValue();
    this.aEntrar.set(true);
    this.erro.set(null);
    this.auth.login(email, password).subscribe({
      next: () => {
        const destino = this.voltar();
        this.router.navigateByUrl(destino && destino.startsWith('/') ? destino : '/');
      },
      error: (err) => {
        this.aEntrar.set(false);
        this.erro.set(
          err?.status === 400 || err?.status === 401
            ? 'E-mail ou palavra-passe incorretos.'
            : mensagemDeErro(err, 'Não foi possível entrar. Tenta novamente.')
        );
      },
    });
  }
}
