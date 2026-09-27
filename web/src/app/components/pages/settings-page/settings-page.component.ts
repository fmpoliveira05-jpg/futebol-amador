import { Component, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';
import { mensagemDeErro } from '../../../shared/http/erros';
import { PALAVRA_PASSE_MAX, REGRA_PALAVRA_PASSE, palavraPasseSegura } from '../../../shared/auth/palavra-passe';

/** As duas palavras-passe novas têm de coincidir. */
function confirmacaoIgual(grupo: AbstractControl): ValidationErrors | null {
  const nova = grupo.get('nova')?.value;
  const confirmacao = grupo.get('confirmacao')?.value;
  return nova && confirmacao && nova !== confirmacao ? { diferentes: true } : null;
}

/**
 * Definições da conta: alteração da palavra-passe e os direitos do RGPD — descarregar os dados
 * (acesso e portabilidade) e eliminar a conta (apagamento, com a palavra-passe).
 */
@Component({
  selector: 'app-settings-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './settings-page.component.html',
})
export class SettingsPageComponent {
  private readonly users = inject(UserService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

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

  // ---------- Os meus dados (RGPD) ----------

  protected readonly aExportar = signal(false);
  protected readonly erroExportar = signal<string | null>(null);

  protected readonly formEliminar = inject(FormBuilder).nonNullable.group({
    palavraPasse: ['', Validators.required],
    confirmo: [false, Validators.requiredTrue],
  });
  protected readonly aEliminar = signal(false);
  protected readonly erroEliminar = signal<string | null>(null);

  /** Descarrega o ficheiro JSON com todos os dados da conta. */
  protected exportar(): void {
    if (this.aExportar()) {
      return;
    }
    this.aExportar.set(true);
    this.erroExportar.set(null);
    this.users.exportarDados().subscribe({
      next: (ficheiro) => {
        this.aExportar.set(false);
        const url = URL.createObjectURL(ficheiro);
        const ligacao = document.createElement('a');
        ligacao.href = url;
        ligacao.download = 'futebol-amador-os-meus-dados.json';
        ligacao.click();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
      },
      error: (e) => {
        this.aExportar.set(false);
        this.erroExportar.set(mensagemDeErro(e, 'Não foi possível exportar os dados.'));
      },
    });
  }

  /** Elimina a conta e termina a sessão (a API apaga os cookies; aqui apaga-se o resto). */
  protected eliminar(): void {
    if (this.formEliminar.invalid) {
      this.formEliminar.markAllAsTouched();
      return;
    }
    if (this.aEliminar()) {
      return;
    }
    this.aEliminar.set(true);
    this.erroEliminar.set(null);
    this.users.eliminarConta(this.formEliminar.getRawValue().palavraPasse).subscribe({
      next: () => {
        this.auth.clearSession();
        try {
          sessionStorage.clear();
        } catch {
          // Sem armazenamento: nada a apagar.
        }
        this.router.navigate(['/'], { queryParams: { conta: 'eliminada' } });
      },
      error: (e) => {
        this.aEliminar.set(false);
        this.erroEliminar.set(mensagemDeErro(e, 'Não foi possível eliminar a conta.'));
      },
    });
  }

  protected invalido(campo: 'atual' | 'nova' | 'confirmacao'): boolean {
    const c = this.form.controls[campo];
    return c.invalid && c.touched;
  }
}
