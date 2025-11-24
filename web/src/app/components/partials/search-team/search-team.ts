import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FilterListTeamDto } from '../../../shared/Dtos/Filters/FilterListTeamDto';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-team',
  imports: [FormsModule],
  templateUrl: './search-team.html',
  styleUrl: './search-team.css',
})
export class SearchTeam {
  filters = new FilterListTeamDto();

  constructor(activatedRoute: ActivatedRoute, private router: Router) {
    activatedRoute.params.subscribe((params) => {
      // if (params.searchTerm) {
      //   this.searchTerm = params.searchTerm;
      // }
    });
  }

  search(filters: FilterListTeamDto): void {
    this.router.navigate(['/teams'], { queryParams: filters });
  }
}
