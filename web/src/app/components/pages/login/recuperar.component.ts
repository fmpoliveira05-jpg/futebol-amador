import { Component, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { mensagemDeErro } from '../../../shared/http/erros';
import { TURNSTILE_ATIVO, TurnstileComponent } from '../../../shared/turnstile/turnstile.component';

/**
 * Recuperação da palavra-passe: a API pede ao Firebase o envio da mensagem. A resposta é sempre a
 * mesma, exista ou não uma conta com o e-mail.
 */
@Component({
  selector: 'app-recuperar',
  imports: [ReactiveFormsModule, RouterLink, TurnstileComponent],
  template: `
    <section class="page page--estreita">
      <header class="page-header">
        <div>
          <h1 class="page-title">Recuperar a palavra-passe</h1>
          <p class="page-subtitle">Enviamos-te um e-mail com uma ligação para escolheres uma palavra-passe nova.</p>
        </div>
      </header>
      <div class="card">
        <form class="form" [formGroup]="form" (ngSubmit)="enviar()" novalidate>
          <div class="form-field">
            <label for="rec-email">E-mail</label>
            <input id="rec-email" formControlName="email" type="email" autocomplete="email" />
            @if (form.controls.email.touched && form.controls.email.invalid) {
              <small class="erro-campo">Introduz um e-mail válido.</small>
            }
          </div>
          <div class="armadilha" aria-hidden="true">
            <label for="rec-website">Não preenchas este campo</label>
            <input id="rec-website" formControlName="website" type="text" tabindex="-1" autocomplete="off" />
          </div>
          <app-turnstile (token)="tokenTurnstile.set($event)" />
          @if (erro()) { <p class="erro" role="alert">{{ erro() }}</p> }
          @if (enviado()) {
            <p class="aviso" role="status">Se existir uma conta com este e-mail, vais receber uma mensagem dentro de alguns minutos.</p>
          }
          <div class="form-actions">
            <button type="submit" class="btn btn-primary" [disabled]="aEnviar()">{{ aEnviar() ? 'A enviar…' : 'Enviar' }}</button>
            <a routerLink="/login" class="btn btn-link">Voltar ao login</a>
          </div>
        </form>
      </div>
    </section>
  `,
})
export class RecuperarComponent {
  private readonly auth = inject(AuthService);
  private readonly turnstile = viewChild(TurnstileComponent);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    website: [''],
  });
  protected readonly aEnviar = signal(false);
  protected readonly enviado = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly tokenTurnstile = signal<string | null>(null);

  protected enviar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, website } = this.form.getRawValue();
    if (website) {
      this.enviado.set(true);
      return;
    }
    if (TURNSTILE_ATIVO && !this.tokenTurnstile()) {
      this.erro.set('Confirma que não és um robô.');
      return;
    }
    this.aEnviar.set(true);
    this.erro.set(null);
    this.auth.recuperarPalavraPasse(email, this.tokenTurnstile(), website).subscribe({
      next: () => {
        this.aEnviar.set(false);
        this.enviado.set(true);
        this.turnstile()?.reiniciar();
      },
      error: (e) => {
        this.aEnviar.set(false);
        this.turnstile()?.reiniciar();
        this.erro.set(mensagemDeErro(e, 'Não foi possível enviar o pedido. Tenta mais tarde.'));
      },
    });
  }
}
