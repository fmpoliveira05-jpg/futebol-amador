import { Component, computed, signal } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { PlayerDetails } from '../../../shared/Dtos/player.model';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TeamService } from '../../../services/team.service';
import { CreateTeamDto } from '../../../shared/Dtos/Team/CreateTeamDto';
import { PitchDto } from '../../../shared/Dtos/Pitch/PitchDto';
import { Router } from '@angular/router';
import { PlayerService } from '../../../services/player.service';

@Component({
  selector: 'app-create-team',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-team.html',
  styleUrl: './create-team.css',
})
export class CreateTeam {
  player = signal<PlayerDetails | null>(null);

  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  constructor(
    private auth: AuthService,
    private fb: FormBuilder,
    private teamService: TeamService,
    private playerService: PlayerService,
    private router: Router
  ) {}

  form!: FormGroup;

  protected readonly hasTeam = computed(() =>
    !!this.player()?.team?.idTeam
  );

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      description: ['', [Validators.required]],
      pitchName: ['', [Validators.required]],
      pitchLocation: ['', [Validators.required]],
    });

    this.loadPlayer();
  }

  loadPlayer(): void {
    this.playerService.getMyProfile().subscribe({
      next: (player) => {
        this.player.set(player);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível carregar as informações do jogador.');
        this.isLoading.set(false);
      }
    })
  }

  protected createTeam(): void {
    const homePitch: PitchDto = {
      name: this.form.value['pitchName'],
      address: this.form.value['pitchLocation'],
    };

    const payload: CreateTeamDto = {
      name: this.form.value['name'],
      description: this.form.value['description'],
      icon: '',
      homePitch: homePitch,
    };

    this.isSaving.set(true);

    this.teamService.createTeam(payload).subscribe({
      next: (response) => {
        this.isSaving.set(false);

        const newTeamId = response?.id;

        if (!newTeamId) {
          this.errorMessage.set('Erro ao obter o ID da nova equipa.');
          return;
        }
        this.loadPlayer();
        this.successMessage.set('Equipa criada com sucesso.');
        this.router.navigate(['/team/details', newTeamId]);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível criar equipa.');
        this.isSaving.set(false);
      },
    });
  }
}
