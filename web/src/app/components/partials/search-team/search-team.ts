import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FilterListTeamDto } from '../../../shared/Dtos/Filters/FilterListTeamDto';
import { FormsModule } from '@angular/forms';

/**
 * Componente responsável pela interface de pesquisa de equipas.
 * Apresenta um formulário de filtros e redireciona o utilizador para a listagem de equipas com os critérios selecionados.
 */
@Component({
  selector: 'app-search-team',
  imports: [FormsModule],
  templateUrl: './search-team.html',
  styleUrl: './search-team.css',
})
export class SearchTeam {
  
  /**
   * Objeto DTO que armazena os critérios de pesquisa inseridos pelo utilizador (ex: nome, cidade).
   * Os campos deste objeto são vinculados aos inputs do formulário via `[(ngModel)]`.
   */
  filters = new FilterListTeamDto();

  /**
   * Construtor do componente.
   * @param router Serviço de roteamento utilizado para navegar para a página de resultados.
   */
  constructor(private router: Router) {}

  /**
   * Executa a ação de pesquisa.
   * Navega para a rota de listagem de equipas (`/teams`), passando os filtros preenchidos como parâmetros de consulta (query params) na URL.
   * Exemplo de URL gerada: `/teams?name=Benfica&city=Lisboa`
   */
  search(): void {
    this.router.navigate(['/teams'], { queryParams: this.filters });
  }
}