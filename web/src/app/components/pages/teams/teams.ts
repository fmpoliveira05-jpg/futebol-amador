import { Component, inject } from '@angular/core';
import { TeamService } from '../../../services/team.service';
import { PlayerService } from '../../../services/player.service';
import { Team } from '../../../shared/models/Team';
import { AuthHeader } from "../../partials/auth-header/auth-header";
import { SearchTeam } from "../../partials/search-team/search-team";
import { NgFor } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-teams',
  imports: [AuthHeader, SearchTeam, NgFor, RouterLink],
  templateUrl: './teams.html',
  styleUrl: './teams.css',
})
export class Teams {
  teams: Team[] = [];

  filter = {
    NameTeam: 'Porto',
    City: '',
    MinAge: 18,
    MaxAge: 30,
    MinNumberPlayers: null,
    MaxNumberPlayers: null,
    NameRank: '',
    MinNumberPoints: null,
    MaxNumberPoints: null,
  };

  constructor(private playerService: PlayerService) {}

  ngOnInit() {
    this.playerService.searchTeams(this.filter).subscribe({
      next: (data) => {
        console.log(data);
        this.teams = data;
      },
      error: (err) => console.error(err),
    });
  }
}
