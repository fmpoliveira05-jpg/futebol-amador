import { Component, inject, signal } from '@angular/core';
import { switchMap } from 'rxjs';
import { mensagemDeErro } from '../../../shared/http/erros';
import { POSICOES } from '../../../shared/constants/position-map';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';

/** Idade mínima para criar conta (`UserConst.MinAge` na API). */
export const IDADE_MINIMA = 18;

/** Valida que a data de nascimento corresponde a pelo menos `IDADE_MINIMA` anos. */
export function idadeMinima(controlo: AbstractControl<string>): ValidationErrors | null {
  if (!controlo.value) {
    return null;
  }
  const nascimento = new Date(controlo.value);
  const limite = new Date();
  limite.setFullYear(limite.getFullYear() - IDADE_MINIMA);
  return nascimento > limite ? { idadeMinima: true } : null;
}

/** Registo de um novo jogador. Depois de criar a conta, entra logo com ela. */
@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  imports: [ReactiveFormsModule, RouterLink],
})
export class SignupComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  /** Mensagem de erro devolvida pela API (em signal: a app não usa zone.js). */
  readonly errorMessage = signal<string | null>(null);
  readonly aGuardar = signal(false);

  /** Os valores seguem o enum `Position` da API. */
  readonly positions = POSICOES;

  readonly signupForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]],
    dateOfBirth: ['', [Validators.required, idadeMinima]],
    address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(250)]],
    phone: ['', [Validators.required, Validators.pattern(/^\+\d{3}\d{9}$/)]],
    position: [0, [Validators.required]],
    height: [175, [Validators.required, Validators.min(100), Validators.max(250)]],
  });

  protected invalido(campo: keyof typeof this.signupForm.controls): boolean {
    const c = this.signupForm.controls[campo];
    return c.invalid && c.touched;
  }

  /** Cria a conta e entra logo com ela. */
  onSignup(): void {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }
    const dados = this.signupForm.getRawValue();
    this.aGuardar.set(true);
    this.errorMessage.set(null);
    this.authService
      .signup({ ...dados, position: Number(dados.position), height: Number(dados.height) })
      .pipe(switchMap(() => this.authService.login(dados.email, dados.password)))
      .subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => {
          this.aGuardar.set(false);
          this.errorMessage.set(mensagemDeErro(err, 'Erro ao criar conta. Tenta novamente.'));
        },
      });
  }
}
