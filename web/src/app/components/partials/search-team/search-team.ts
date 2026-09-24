import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { FilterListTeamDto } from '../../../shared/Dtos/Filters/FilterListTeamDto';

/** Formulário de filtros da lista de equipas. Os filtros vão para o URL (`?NameTeam=...`). */
@Component({
  selector: 'app-search-team',
  imports: [FormsModule],
  templateUrl: './search-team.html',
  styleUrl: './search-team.css',
})
export class SearchTeam {
  private readonly router = inject(Router);

  filters = new FilterListTeamDto();

  search(): void {
    this.router.navigate(['/teams'], { queryParams: { ...this.filters } });
  }

  limpar(): void {
    this.filters = new FilterListTeamDto();
    this.router.navigate(['/teams']);
  }
}
