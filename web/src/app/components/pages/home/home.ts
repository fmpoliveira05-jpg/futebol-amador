import { Component } from '@angular/core';
import { PlayerService } from '../../../services/player.service';
import { TeamService } from '../../../services/team.service';
import { InfoTeamDto } from '../../../shared/Dtos/Team/InfoTeamDto';
import { RouterLink } from '@angular/router';
import { Header } from '../../partials/header/header';

@Component({
  selector: 'app-home',
  imports: [RouterLink, Header],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {

  teams:InfoTeamDto[] = [];
  
  constructor(private playerService:PlayerService, private teamService:TeamService){
    // this.teams = teamService.getAll();
  }

}
