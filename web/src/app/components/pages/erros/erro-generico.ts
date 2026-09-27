import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Página de erro genérica (`/erro`). O motivo vem em `?motivo=`: `atualizacao` quando uma parte da
 * aplicação não carregou (normalmente porque saiu uma versão nova) e `inesperado` para o resto.
 * Nunca mostra detalhes técnicos.
 */
@Component({
  selector: 'app-erro-generico',
  imports: [RouterLink],
  template: `
    <section class="page">
      <header class="page-header">
        <div>
          <h1 class="page-title">{{ titulo() }}</h1>
        </div>
      </header>
      <div class="card">
        <p role="alert">{{ texto() }}</p>
        <p>
          <button type="button" class="btn btn-primary" (click)="recarregar()">Recarregar</button>
          <a class="btn btn-link" routerLink="/">Página inicial</a>
        </p>
      </div>
    </section>
  `,
})
export class ErroGenerico {
  readonly motivo = input<string | undefined>();

  protected readonly titulo = computed(() =>
    this.motivo() === 'atualizacao' ? 'Há uma versão nova' : 'Algo correu mal'
  );
  protected readonly texto = computed(() =>
    this.motivo() === 'atualizacao'
      ? 'Não foi possível carregar esta parte da aplicação, provavelmente porque saiu uma versão nova. Recarrega a página.'
      : 'Ocorreu um erro inesperado. Recarrega a página; se continuar, tenta mais tarde.'
  );

  protected recarregar(): void {
    location.assign('/');
  }
}
