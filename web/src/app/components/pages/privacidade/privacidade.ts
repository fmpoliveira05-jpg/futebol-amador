import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { environment } from '../../../environments/environment';
import { VERSAO_POLITICA_PRIVACIDADE } from '../../../shared/rgpd/politica';

/**
 * Política de Privacidade (RGPD, art. 13.º). Pública: tem de poder ser lida antes do registo.
 * O registo de atividades de tratamento e os pormenores técnicos estão em docs/RGPD.md.
 */
@Component({
  selector: 'app-privacidade',
  imports: [RouterLink],
  templateUrl: './privacidade.html',
})
export class Privacidade {
  protected readonly versao = VERSAO_POLITICA_PRIVACIDADE;
  protected readonly contacto = environment.contactoPrivacidade;
}
