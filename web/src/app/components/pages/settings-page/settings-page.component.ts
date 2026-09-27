import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { mensagemDeErro } from '../../../shared/http/erros';
import { PALAVRA_PASSE_MAX, REGRA_PALAVRA_PASSE, palavraPasseSegura } from '../../../shared/auth/palavra-passe';

/** As duas palavras-passe novas têm de coincidir. */
function confirmacaoIgual(grupo: AbstractControl): ValidationErrors | null {
  const nova = grupo.get('nova')?.value;
  const confirmacao = grupo.get('confirmacao')?.value;
  return nova && confirmacao && nova !== confirmacao ? { diferentes: true } : null;
}

/** Definições da conta: alteração da palavra-passe. */
@Component({
  selector: 'app-settings-page',
  imports: [ReactiveFormsModule],
  templateUrl: './settings-page.component.html',
})
export class SettingsPageComponent {
  private readonly users = inject(UserService);

  protected readonly form = inject(FormBuilder).nonNullable.group(
    {
      atual: ['', Validators.required],
      nova: ['', [Validators.required, Validators.maxLength(PALAVRA_PASSE_MAX), palavraPasseSegura]],
      confirmacao: ['', Validators.required],
    },
    { validators: confirmacaoIgual }
  );

  protected readonly regraPalavraPasse = REGRA_PALAVRA_PASSE;
  protected readonly aGuardar = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly sucesso = signal(false);

  protected guardar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { atual, nova } = this.form.getRawValue();
    this.aGuardar.set(true);
    this.erro.set(null);
    this.sucesso.set(false);

    this.users.alterarPalavraPasse(atual, nova).subscribe({
      next: () => {
        this.aGuardar.set(false);
        this.sucesso.set(true);
        this.form.reset();
      },
      error: (e) => {
        this.aGuardar.set(false);
        this.erro.set(mensagemDeErro(e, 'Não foi possível alterar a palavra-passe.'));
      },
    });
  }

  protected invalido(campo: 'atual' | 'nova' | 'confirmacao'): boolean {
    const c = this.form.controls[campo];
    return c.invalid && c.touched;
  }
}
