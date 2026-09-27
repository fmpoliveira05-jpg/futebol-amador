import { Component, inject, signal, viewChild } from '@angular/core';
import { VERSAO_POLITICA_PRIVACIDADE } from '../../../shared/rgpd/politica';
import { mensagemDeErro } from '../../../shared/http/erros';
import { POSICOES } from '../../../shared/constants/position-map';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { PALAVRA_PASSE_MAX, REGRA_PALAVRA_PASSE, palavraPasseSegura } from '../../../shared/auth/palavra-passe';
import { TURNSTILE_ATIVO, TurnstileComponent } from '../../../shared/turnstile/turnstile.component';

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

/**
 * Registo de um novo jogador. A API envia a confirmação do e-mail: normalmente só se entra depois
 * de a confirmar (volta ao login com um aviso). Se a API não a exigir, a sessão começa logo.
 */
@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  imports: [ReactiveFormsModule, RouterLink, TurnstileComponent],
})
export class SignupComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  /** Mensagem de erro devolvida pela API (em signal: a app não usa zone.js). */
  readonly errorMessage = signal<string | null>(null);
  readonly aGuardar = signal(false);

  protected readonly regraPalavraPasse = REGRA_PALAVRA_PASSE;
  readonly tokenTurnstile = signal<string | null>(null);
  private readonly turnstile = viewChild(TurnstileComponent);

  /** Os valores seguem o enum `Position` da API. */
  readonly positions = POSICOES;

  readonly signupForm = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.maxLength(PALAVRA_PASSE_MAX), palavraPasseSegura]],
    dateOfBirth: ['', [Validators.required, idadeMinima]],
    address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(250)]],
    phone: ['', [Validators.required, Validators.pattern(/^\+\d{3}\d{9}$/)]],
    position: [0, [Validators.required]],
    height: [175, [Validators.required, Validators.min(100), Validators.max(250)]],
    /** Aceitação da Política de Privacidade (obrigatória; a API guarda a versão e a data). */
    aceitaPolitica: [false, [Validators.requiredTrue]],
    /** Campo-armadilha, escondido das pessoas. */
    website: [''],
  });

  protected invalido(campo: keyof typeof this.signupForm.controls): boolean {
    const c = this.signupForm.controls[campo];
    return c.invalid && c.touched;
  }

  /** Cria a conta; depois pede a confirmação do e-mail (ou entra, se a API não a exigir). */
  onSignup(): void {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }
    const { aceitaPolitica, ...dados } = this.signupForm.getRawValue();
    if (dados.website) {
      // Só um robô preenche o campo escondido: não se chama a API.
      return;
    }
    if (TURNSTILE_ATIVO && !this.tokenTurnstile()) {
      this.errorMessage.set('Confirma que não és um robô.');
      return;
    }
    this.aGuardar.set(true);
    this.errorMessage.set(null);
    this.authService
      .signup(
        {
          ...dados,
          position: Number(dados.position),
          height: Number(dados.height),
          aceitaPoliticaPrivacidade: aceitaPolitica,
          versaoPoliticaPrivacidade: VERSAO_POLITICA_PRIVACIDADE,
        },
        this.tokenTurnstile()
      )
      .subscribe({
        next: (resposta) => {
          if ('verificacaoEmailPendente' in resposta && resposta.verificacaoEmailPendente) {
            this.router.navigate(['/login'], { queryParams: { registo: 'pendente' } });
          } else {
            this.router.navigate(['/']);
          }
        },
        error: (err) => {
          this.aGuardar.set(false);
          this.turnstile()?.reiniciar();
          this.errorMessage.set(mensagemDeErro(err, 'Erro ao criar conta. Tenta novamente.'));
        },
      });
  }
}
