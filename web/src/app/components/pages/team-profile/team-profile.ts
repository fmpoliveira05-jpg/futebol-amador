import { Component, computed, inject, signal } from '@angular/core';
import { InfoTeamDto } from '../../../shared/Dtos/Team/InfoTeamDto';
import { ActivatedRoute } from '@angular/router';
import { TeamService } from '../../../services/team.service';
import { CommonModule } from '@angular/common';
import { TeamDetailsDto } from '../../../shared/Dtos/Team/TeamDetailsDto';

@Component({
  selector: 'app-team-profile',
  imports: [CommonModule],
  templateUrl: './team-profile.html',
  styleUrl: './team-profile.css',
})
export class TeamProfile {
  team =  signal<TeamDetailsDto | null>(null);

  isLoading = signal<boolean>(false);

  constructor(private teamService: TeamService, private route: ActivatedRoute) {}

  ngOnInit() {
    this.route.params.subscribe((params) => {
      this.loadTeam(params['teamId']);
    });
  }

  loadTeam(teamId: string) {
    this.isLoading.set(true);

    this.teamService.getTeamById(teamId).subscribe({
      next: (data) => {
        this.team.set(data);
        this.isLoading.set(false);
        console.log(data);
      },
      error: (err) => {
        console.error(err);
      },
    });
  }
}
