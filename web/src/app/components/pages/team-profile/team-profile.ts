import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TeamService } from '../../../services/team.service';
import { CommonModule, NgFor } from '@angular/common';
import { TeamDetailsDto } from '../../../shared/Dtos/Team/TeamDetailsDto';
import { PlayerService } from '../../../services/player.service';
import { PlayerDetails } from '../../../shared/Dtos/player.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { UpdateTeamDto } from '../../../shared/Dtos/Team/UpdateTeamDto';

@Component({
  selector: 'app-team-profile',
  imports: [CommonModule, NgFor, ReactiveFormsModule],
  templateUrl: './team-profile.html',
  styleUrl: './team-profile.css',
})
export class TeamProfile {
  team = signal<TeamDetailsDto | null>(null);
  currentUser = signal<PlayerDetails | null>(null);

  protected form!: FormGroup;

  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isLoading = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  constructor(
    private teamService: TeamService,
    private route: ActivatedRoute,
    private router: Router,
    private playerService: PlayerService,
    private fb: FormBuilder
  ) {}

  ngOnInit() {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]],
      pitchName: ['', [Validators.required]],
      pitchLocation: ['', [Validators.required]],
    });

    this.playerService.getMyProfile().subscribe({
      next: (user) => this.currentUser.set(user),
      error: (err) => console.error(err),
    });

    this.route.params.subscribe((params) => {
      this.loadTeam(params['teamId']);
    });
  }

  isMember = computed(() => {
    const team = this.team();
    const user = this.currentUser();

    if (!team || !user) return false;

    const player = team.players.find((p) => p.playerId === user.playerId);

    console.log(user.playerId + ' = ' + player?.playerId);

    return team.players.some((p) => p.playerId === user.playerId);
  });

  isAdmin = computed(() => {
    const team = this.team();
    const user = this.currentUser();

    if (!team || !user) return false;

    const member = team.players.find((p) => p.playerId === user.playerId);
    return member?.isAdmin === true;
  });

  protected goToTeamMembers(): void {
    this.router.navigate(['/team/members']);
  }

  protected toggleEdit(): void {
    this.isEditMode.set(!this.isEditMode());
    this.successMessage.set(null);
    this.errorMessage.set(null);

    const t = this.team();
    if (this.isEditMode() && t) {
      this.form.patchValue({
        name: t.name,
        description: t.description,
        pitchName: t.pitchDto.name,
        pitchLocation: t.pitchDto.address,
      });
    }
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

  protected save(): void {
    const team = this.team();

    if (!team || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: UpdateTeamDto = {
      name: this.form.value['name'],
      description: this.form.value['description'],
      pitchName: this.form.value['pitchName'],
      pitchLocation: this.form.value['pitchAddress'],
    };

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.teamService.updateTeam(team.id, payload).subscribe({
      next: () => {
        this.successMessage.set('Equipa atualizada com sucesso.');
        this.isSaving.set(false);
        this.isEditMode.set(false);
        this.loadTeam(team.id); // reload updated team data
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível guardar as alterações.');
        this.isSaving.set(false);
      },
    });
  }

  protected deleteTeam(): void {
    const t = this.team();
    if (!t) return;

    if (
      !confirm('Tens a certeza absoluta que queres apagar a tua equipa? Esta ação é irreversível.')
    ) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.teamService.deleteTeam(t.id).subscribe({
      next: () => {
        this.isSaving.set(false);
        alert('Equipa apagada com sucesso. Serás redirecionado.');
        this.router.navigate(['/teams']);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível apagar a equipa.');
        this.isSaving.set(false);
      },
    });
  }
}
