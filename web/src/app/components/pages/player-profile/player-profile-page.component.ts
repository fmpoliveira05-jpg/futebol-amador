import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PlayerDetails, UpdatePlayerRequest } from '../../../shared/Dtos/player.model';
import { PlayerService } from '../../../services/player.service';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { POSITION_MAP_TONUMBER } from '../../../shared/constants/position-map-to-number';
import { POSITION_MAP } from '../../../shared/constants/position-map';

/**
 * Componente responsável pela gestão do perfil do jogador.
 * Permite visualizar, editar, e atualizar o perfil do jogador, bem como gerir a adesão à equipa e a exclusão de conta.
 */
@Component({
  selector: 'app-player-profile-page',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './player-profile-page.component.html',
  styleUrl: './player-profile-page.component.css',
})
export class PlayerProfilePageComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute); // Serviço para capturar parâmetros da rota
  private readonly router = inject(Router); // Serviço para navegação
  private readonly playerService = inject(PlayerService); // Serviço de jogador
  private readonly fb = inject(FormBuilder); // Serviço para construção de formulários reativos

  protected readonly isLoading = signal<boolean>(false); // Estado de carregamento
  protected readonly isSaving = signal<boolean>(false); // Estado de salvamento
  protected readonly errorMessage = signal<string | null>(null); // Mensagem de erro
  protected readonly successMessage = signal<string | null>(null); // Mensagem de sucesso
  protected readonly isEditMode = signal<boolean>(false); // Estado de edição do perfil
  protected readonly player = signal<PlayerDetails | null>(null); // Dados do jogador
  protected readonly POSITION_MAP_TONUMBER = POSITION_MAP_TONUMBER; // Mapeamento de posições para valores numéricos
  protected readonly POSITION_MAP = POSITION_MAP; // Mapeamento de posições

  private auth = inject(AuthService); // Serviço de autenticação

  protected form!: FormGroup; // Formulário de edição de perfil

  private sub?: Subscription; // Assinatura do parâmetro da rota

  protected readonly hasTeam = computed(() => !!this.player()?.idTeam); // Verifica se o jogador pertence a uma equipa
  protected readonly isOwnProfile = computed(() => this.player()?.playerId === this.auth.getCurrentPlayerId()); // Verifica se o perfil é do próprio jogador

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

  /**
   * Alterna entre o modo de visualização e edição do perfil do jogador.
   */
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

  /**
   * Salva as alterações feitas no perfil do jogador.
   */
  protected save(): void {
    if (!this.player() || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const current = this.player()!;
    const position = this.form.value['position'];
    const positionEnumValue = this.POSITION_MAP_TONUMBER[position];

    if (positionEnumValue === undefined) {
      this.errorMessage.set('Valor de posição inválido.');
      return;
    }

    const payload = {
      playerId: current.playerId,
      Name: this.form.value['name'] || current.name,
      DateOfBirth: this.form.value['dateOfBirth'] || current.dateOfBirth,
      Address: this.form.value['address'] || current.address,
      Email: this.form.value['email'] || current.email,
      Phone: this.form.value['phone'] || current.phoneNumber,
      Position: positionEnumValue,
      Height: Number(this.form.value['height']) || current.heigth,
    };

    if (!payload.Name || !payload.Email || !payload.Phone || !payload.Address || !payload.Height) {
      this.errorMessage.set('Campos obrigatórios não preenchidos.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.playerService.updatePlayer(payload.playerId, payload).subscribe({
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

  /**
   * Permite ao jogador abandonar a sua equipa.
   */
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

  /**
   * Apaga a conta do jogador de forma irreversível.
   */
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

  /**
   * Redireciona para a página de pedidos de adesão, dependendo se o jogador tem ou não equipa.
   */
  protected onViewMembershipRequests(): void {
    const player = this.player();
    
    if (!player) {
      return;
    }

    if (player.idTeam == null) {
      this.router.navigate(['/players/membership-requests']);
    } else {
      this.router.navigate(['/team/membership-requests']);
    }
  }

  /**
   * Método fictício para enviar um pedido de adesão a uma equipa.
   */
  protected onSendMembershipRequest(): void {
    alert('Aqui, no futuro, vais enviar um pedido de adesão a uma equipa.');
  }

  goToTeamProfile(): void{
    this.router.navigate(['/team/details', this.player()?.idTeam]);
  }
  
  /**
   * Carrega os dados de um jogador a partir do ID ou carrega o perfil do jogador autenticado.
   */
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