import { ErrorHandler, Injectable, NgZone, inject } from '@angular/core';
import { Router } from '@angular/router';

/**
 * Erros não tratados: ficam na consola (sem dados pessoais) e, se uma parte da aplicação não
 * carregou (ficheiro JavaScript de uma versão anterior, sem rede), abre a página `/erro`.
 * Os erros HTTP são tratados em cada página (estados de erro) e não chegam aqui.
 */
@Injectable()
export class TratadorErros implements ErrorHandler {
  private readonly router = inject(Router);
  private readonly zone = inject(NgZone);

  handleError(erro: unknown): void {
    console.error(erro);
    const mensagem = erro instanceof Error ? erro.message : String(erro ?? '');
    if (/Failed to fetch dynamically imported module|Loading chunk|Importing a module script failed/i.test(mensagem)) {
      this.zone.run(() => this.router.navigate(['/erro'], { queryParams: { motivo: 'atualizacao' } }));
    }
  }
}
