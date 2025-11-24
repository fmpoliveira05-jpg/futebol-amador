import { Component, inject } from '@angular/core';
import { PlayerService } from '../../../services/player.service';
import { InfoTeamDto } from '../../../shared/Dtos/Team/InfoTeamDto';
import { AuthHeader } from '../../partials/auth-header/auth-header';
import { SearchTeam } from '../../partials/search-team/search-team';
import { NgFor } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FilterListTeamDto } from '../../../shared/Dtos/Filters/FilterListTeamDto';
import { InfoRankDto } from '../../../shared/Dtos/Rank/InfoRankDto';

@Component({
  selector: 'app-teams',
  imports: [AuthHeader, SearchTeam, NgFor, RouterLink],
  templateUrl: './teams.html',
  styleUrl: './teams.css',
})
export class Teams {
  teams: InfoTeamDto[] = [];

  filter?: FilterListTeamDto;

  constructor(private playerService: PlayerService) {}

  ngOnInit() {
    this.loadTeams();
    // this.teams   = sample_teams;
  }

  loadTeams() {
    this.playerService.searchTeams(this.filter).subscribe({
      next: (data) => {
        console.log('Teams from backend:', data);
        this.teams = data;
      },
      error: (err) => {
        console.error(err)
      },
    });
  }

  applyFilter(newFilter: FilterListTeamDto) {
    this.filter = { ...this.filter, ...newFilter };
    this.loadTeams();
  }
}

const sample_teams: InfoTeamDto[] = [
  {
    id: '01',
    name: 'AASIO FC',
    description: 'aaa',
    address: 'rua voliuntarions da patrai',
    rank: new InfoRankDto(),
    currentPoints: 300,
    averageAge: 22,
    playerCount: 12,
  },
  {
    id: '02',
    name: 'Botafogo',
    description: 'fogao',
    address: 'largo dos leoes',
    rank: new InfoRankDto(),
    currentPoints: 200,
    averageAge: 26,
    playerCount: 13,
  },
  {
    id: '03',
    name: 'Vasco',
    description: 'vascao',
    address: 'rua vascoc',
    rank: new InfoRankDto(),
    currentPoints: 100,
    averageAge: 25,
    playerCount: 15,
  }
];
