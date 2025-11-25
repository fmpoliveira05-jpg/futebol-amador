import { Component, signal } from '@angular/core';
import { PlayerService } from '../../../services/player.service';
import { InfoTeamDto } from '../../../shared/Dtos/Team/InfoTeamDto';
import { FilterListTeamDto } from '../../../shared/Dtos/Filters/FilterListTeamDto';
import { CommonModule} from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { SearchTeam } from '../../partials/search-team/search-team';

@Component({
  selector: 'app-teams',
  imports: [SearchTeam, CommonModule, RouterLink],
  templateUrl: './teams.html',
  styleUrl: './teams.css',
})
export class Teams {
  teams = signal<InfoTeamDto[]>([]);
  isLoading = signal<boolean>(false);
  errorMessage = signal<string | null>(null);

  filter = signal<FilterListTeamDto>({});

  constructor(private playerService: PlayerService, private route: ActivatedRoute) {}

  ngOnInit() {
    this.route.queryParams.subscribe((params) => {
      this.filter.set(params as FilterListTeamDto);
      this.loadTeams();
    });
  }

  loadTeams() {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.playerService.searchTeams(this.filter()).subscribe({
      next: (data) => {
        this.teams.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Error loading teams');
        this.isLoading.set(false);
      },
    });
  }

  applyFilter(newFilter: FilterListTeamDto) {
    this.filter.set(newFilter);
    this.loadTeams();
  }
}
