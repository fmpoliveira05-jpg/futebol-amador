import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/** Página para endereços que não existem (rota `**`). */
@Component({
  selector: 'app-nao-encontrada',
  imports: [RouterLink],
  template: `
    <section class="page">
      <header class="page-header">
        <div>
          <p class="page-subtitle">Erro 404</p>
          <h1 class="page-title">Página não encontrada</h1>
        </div>
      </header>
      <div class="card">
        <p>O endereço que abriste não existe ou mudou de sítio.</p>
        <p><a class="btn btn-primary" routerLink="/">Voltar à página inicial</a></p>
      </div>
    </section>
  `,
})
export class NaoEncontrada {}
