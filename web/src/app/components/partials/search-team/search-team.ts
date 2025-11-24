import { Component } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-search-team',
  imports: [],
  templateUrl: './search-team.html',
  styleUrl: './search-team.css',
})
export class SearchTeam {
  searchTerm = '';

  constructor(activatedRoute: ActivatedRoute, private router: Router) {
    activatedRoute.params.subscribe((params) => {
      // if (params.searchTerm) {
      //   this.searchTerm = params.searchTerm;
      // }
    });
  }
}
