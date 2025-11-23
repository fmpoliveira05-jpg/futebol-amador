import { Component } from '@angular/core';
import { PlayerService } from '../../../services/player.service';
import { TeamService } from '../../../services/team.service';
import { Team } from '../../../shared/models/Team';
import { RouterLink } from '@angular/router';
import { Header } from '../../partials/header/header';

@Component({
  selector: 'app-home',
  imports: [RouterLink, Header],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {

  teams:Team[] = [];
  
  constructor(private playerService:PlayerService, private teamService:TeamService){
    // this.teams = teamService.getAll();
  }

}
