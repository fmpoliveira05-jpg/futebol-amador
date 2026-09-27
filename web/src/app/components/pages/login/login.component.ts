import { Component, inject, input, signal, viewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { mensagemDeErro } from '../../../shared/http/erros';
import { environment } from '../../../environments/environment';
import { TURNSTILE_ATIVO, TurnstileComponent } from '../../../shared/turnstile/turnstile.component';

/** Código devolvido pela API quando o e-mail ainda não foi confirmado. */
export const EMAIL_NAO_VERIFICADO = 'email_nao_verificado';

/** Página de login. Depois de entrar volta à página pedida (`?voltar=`) ou ao perfil. */
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  imports: [ReactiveFormsModule, RouterLink, TurnstileComponent],
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  /** Parâmetros de query (ligados pelo router). */
  readonly voltar = input<string>();
  readonly sessao = input<string>();
  readonly registo = input<string>();

  /** No modo demonstração qualquer e-mail e palavra-passe servem. */
  protected readonly demo = environment.demo;
  protected readonly turnstileAtivo = TURNSTILE_ATIVO;
  private readonly turnstile = viewChild(TurnstileComponent);

  readonly aEntrar = signal(false);
  readonly erro = signal<string | null>(null);
  readonly info = signal<string | null>(null);
  /** O login falhou por o e-mail não estar confirmado: mostra o botão de reenvio. */
  readonly porConfirmar = signal(false);
  readonly tokenTurnstile = signal<string | null>(null);

  readonly loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.maxLength(128)]],
    /** Campo-armadilha, escondido das pessoas. */
    website: [''],
  });

  onLogin(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }
    const { email, password, website } = this.loginForm.getRawValue();
    if (website) {
      // Só um robô preenche o campo escondido: não se chama a API.
      return;
    }
    if (this.turnstileAtivo && !this.tokenTurnstile()) {
      this.erro.set('Confirma que não és um robô.');
      return;
    }
    this.aEntrar.set(true);
    this.erro.set(null);
    this.info.set(null);
    this.porConfirmar.set(false);
    this.auth.login(email, password, this.tokenTurnstile()).subscribe({
      next: () => {
        const destino = this.voltar();
        this.router.navigateByUrl(destino && destino.startsWith('/') && !destino.startsWith('//') ? destino : '/');
      },
      error: (err) => {
        this.aEntrar.set(false);
        this.turnstile()?.reiniciar();
        if (err instanceof HttpErrorResponse && err.status === 403 && err.error?.codigo === EMAIL_NAO_VERIFICADO) {
          this.porConfirmar.set(true);
          this.erro.set('Ainda não confirmaste o teu e-mail. Abre a mensagem que te enviámos.');
          return;
        }
        this.erro.set(
          err?.status === 400 || err?.status === 401
            ? 'E-mail ou palavra-passe incorretos.'
            : mensagemDeErro(err, 'Não foi possível entrar. Tenta novamente.')
        );
      },
    });
  }

  /** Volta a enviar a confirmação do e-mail (a resposta da API é sempre a mesma). */
  reenviarConfirmacao(): void {
    const { email, password } = this.loginForm.getRawValue();
    this.aEntrar.set(true);
    this.auth.reenviarConfirmacao(email, password, this.tokenTurnstile()).subscribe({
      next: () => {
        this.aEntrar.set(false);
        this.porConfirmar.set(false);
        this.erro.set(null);
        this.info.set('Se os dados estiverem certos, vais receber um e-mail dentro de alguns minutos.');
        this.turnstile()?.reiniciar();
      },
      error: (err) => {
        this.aEntrar.set(false);
        this.turnstile()?.reiniciar();
        this.erro.set(mensagemDeErro(err, 'Não foi possível reenviar a mensagem.'));
      },
    });
  }
}
