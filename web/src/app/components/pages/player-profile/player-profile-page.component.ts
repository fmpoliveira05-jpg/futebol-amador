import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PlayerDetails, UpdatePlayerRequest } from '../../../shared/Dtos/player.model';
import { PlayerService } from '../../../services/player.service';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { POSITION_MAP } from '../../../shared/constants/position-map';

@Component({
  selector: 'app-player-profile-page',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './player-profile-page.component.html',
  styleUrl: './player-profile-page.component.css',
})
export class PlayerProfilePageComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly playerService = inject(PlayerService);
  private readonly fb = inject(FormBuilder);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isSaving = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);
  protected readonly isEditMode = signal<boolean>(false);
  protected readonly player = signal<PlayerDetails | null>(null);
  protected readonly POSITION_MAP = POSITION_MAP;

  private auth = inject(AuthService);

  protected form!: FormGroup;

  private sub?: Subscription;

  protected readonly hasTeam = computed(() =>
    !!this.player()?.idTeam
  );

  protected readonly isOwnProfile = computed(() =>
    this.player()?.playerId === this.auth.getCurrentPlayerId()
  );

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required]],
      address: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required]],
      position: ['', [Validators.required]],
      height: ['', [Validators.required]],
    });

    this.sub = this.route.paramMap.subscribe((params) => {
      const id = params.get('playerId');
      if (!id) {
        this.router.navigate(['/players']);
        return;
      }

      this.loadPlayer(id);
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  protected toggleEdit(): void {
    this.isEditMode.set(!this.isEditMode());
    this.successMessage.set(null);
    this.errorMessage.set(null);

    const p = this.player();
    if (this.isEditMode() && p) {
      this.form.patchValue({
        name: p.name,
        dateOfBirth: p.dateOfBirth,
        address: p.address,
        email: p.email,
        phone: p.phoneNumber,
        position: POSITION_MAP[p.position],
        height: p.height,
      });
    }
  }

  protected goToTeamMembers(): void {
    this.router.navigate(['/team/members']);
  }

  protected save(): void {
    if (!this.player() || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const current = this.player()!;
    const payload: UpdatePlayerRequest = {
      playerId: current.playerId,
      name: this.form.value['name'],
      dateOfBirth: this.form.value['dateOfBirth'],
      address: this.form.value['address'],
      email: this.form.value['email'],
      phone: this.form.value['phone'],
      position: this.form.value['position'],
      height: Number(this.form.value['height']),
    };

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.playerService.updatePlayer(current.playerId, payload).subscribe({
      next: () => {
        this.successMessage.set('Perfil atualizado com sucesso.');
        this.isSaving.set(false);
        this.isEditMode.set(false);
        this.loadPlayer(current.playerId, false);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível guardar as alterações.');
        this.isSaving.set(false);
      },
    });
  }

  protected leaveTeam(): void {
    const p = this.player();
    if (!p || !p.idTeam) {
      return;
    }

    if (!confirm('Tens a certeza que queres abandonar a equipa?')) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.playerService.leaveTeam(p.playerId).subscribe({
      next: () => {
        this.successMessage.set('Saíste da equipa com sucesso.');
        this.isSaving.set(false);
        this.loadPlayer(p.playerId, false);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível abandonar a equipa.');
        this.isSaving.set(false);
      },
    });
  }

  protected deleteAccount(): void {
    const p = this.player();
    if (!p) return;

    if (
      !confirm(
        'Tens a certeza absoluta que queres apagar a tua conta? Esta ação é irreversível.',
      )
    ) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.playerService.deletePlayer(p.playerId).subscribe({
      next: () => {
        this.isSaving.set(false);
        alert('Conta apagada com sucesso. Serás redirecionado.');
        this.router.navigate(['/players']);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível apagar a conta.');
        this.isSaving.set(false);
      },
    });
  }

  protected onViewMembershipRequests(): void {
    const player = this.player();
    
    if (!player) {
      return;
    }

    if (!player.idTeam) {
      this.router.navigate(['/players/membership-requests']);
    } else if (player.isAdmin) {
      this.router.navigate(['/team/membership-requests']);
    } else {
      this.errorMessage.set('Apenas administradores de equipa podem gerir pedidos de adesão.');
    }
  }

  protected onSendMembershipRequest(): void {
    alert('Aqui, no futuro, vais enviar um pedido de adesão a uma equipa.');
  }

  private loadPlayer(playerIdOrMe: string, showLoader: boolean = true): void {
    if (showLoader) {
      this.isLoading.set(true);
    }
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const observable =
      playerIdOrMe === 'me'
        ? this.playerService.getMyProfile()
        : this.playerService.getPlayerById(playerIdOrMe);

    observable.subscribe({
      next: (player) => {
        this.player.set(player);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível carregar o perfil do jogador.');
        this.isLoading.set(false);
      },
    });
  }
}