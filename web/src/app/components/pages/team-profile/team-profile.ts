import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TeamService } from '../../../services/team.service';
import { CommonModule, NgFor } from '@angular/common';
import { TeamDetailsDto } from '../../../shared/Dtos/Team/TeamDetailsDto';
import { PlayerService } from '../../../services/player.service';
import { PlayerDetails } from '../../../shared/Dtos/player.model';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MembershipRequestService } from '../../../services/membership-request.service';
import { CreateTeamDto } from '../../../shared/Dtos/Team/CreateTeamDto';
import { PitchDto } from '../../../shared/Dtos/Pitch/PitchDto';

/**
 * Componente responsável por exibir e gerir o perfil de uma equipa.
 * Permite visualizar detalhes, editar informações (se admin), e gerir ações como pedidos de adesão ou exclusão da equipa.
 */
@Component({
  selector: 'app-team-profile',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './team-profile.html',
  styleUrl: './team-profile.css',
})
export class TeamProfile {
  team = signal<TeamDetailsDto | null>(null);
  currentUser = signal<PlayerDetails | null>(null);
  tempIcon = signal<string | null>(null);

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
    private fb: FormBuilder,
    private membershipRequestService: MembershipRequestService
  ) {}

  /**
   * Inicializa o componente, configurando o formulário de edição e carregando os dados do jogador e da equipa (via rota).
   */
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

  /**
   * Computada: Verifica se o utilizador logado pertence a alguma equipa.
   */
  hasTeam = computed(() => {
    const user = this.currentUser();
    return user?.team;
  })

  /**
   * Computada: Verifica se o utilizador logado é membro desta equipa específica.
   */
  isMember = computed(() => {
    const team = this.team();
    const user = this.currentUser();

    if (!team || !user) return false;
    return team.players.some((p) => p.playerId === user.playerId);
  });

  /**
   * Computada: Verifica se o utilizador logado é administrador.
   */
  isAdmin = computed(() => {
    const user = this.currentUser();
    return user?.isAdmin === true;
  });

  /**
   * Navega para a página de gestão de membros da equipa.
   */
  protected goToTeamMembers(): void {
    this.router.navigate(['/team/members']);
  }

  /**
   * Navega para a página de convites de partida.
   */
  protected goToMatchInvites(): void {
    this.router.navigate(['/team/matchInvites']);
  }

  /**
   * Alterna entre o modo de visualização e o modo de edição do perfil da equipa.
   * Preenche o formulário com os dados atuais ao entrar no modo de edição.
   */
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
      this.tempIcon.set(null);
    } else {
      this.tempIcon.set(null);
    }
  }

  /**
   * Carrega os dados da equipa através do ID fornecido.
   * @param teamId ID da equipa.
   */
  loadTeam(teamId: string) {
    this.isLoading.set(true);

    this.teamService.getTeamById(teamId).subscribe({
      next: (data) => {
        this.team.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
      },
    });
  }

  /**
   * Processa a seleção de um ficheiro de imagem (ícone da equipa).
   * Converte a imagem para Base64 para pré-visualização e envio.
   * @param event Evento de input do ficheiro.
   */
  protected onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const reader = new FileReader();

      reader.onload = () => {
        this.tempIcon.set(reader.result as string);
      };

      reader.onerror = (error) => {
        console.error('Error reading file:', error);
        this.errorMessage.set('Erro ao carregar a imagem.');
      };

      reader.readAsDataURL(file);
    }
  }

  /**
   * Guarda as alterações feitas no perfil da equipa.
   * Envia os dados do formulário e a imagem (se alterada) para a API.
   */
  protected save(): void {
    const team = this.team();

    if (!team || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const homePitch: PitchDto = {
      name: this.form.value['pitchName'],
      address: this.form.value['pitchLocation'],
    };

    const base64Icon = this.tempIcon()
      ? this.tempIcon()?.split(',')[1]
      : null;

    const payload: CreateTeamDto = {
      name: this.form.value['name'],
      description: this.form.value['description'],
      icon: base64Icon,
      homePitch: homePitch,
    };

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.teamService.updateTeam(team.id, payload).subscribe({
      next: () => {
        this.successMessage.set('Equipa atualizada com sucesso.');
        this.isSaving.set(false);
        this.isEditMode.set(false);
        this.tempIcon.set(null);
        this.loadTeam(team.id); // reload updated team data
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível guardar as alterações.');
        this.isSaving.set(false);
      },
    });
  }

  /**
   * Envia um pedido para aderir a esta equipa.
   */
  protected sendMembershipRequest(): void {
    const t = this.team();
    if (!t) return;

    if (!confirm('Confirma envio de pedido de adesão à equipa?')) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.membershipRequestService.sendMembershipRequestPlayer(this.team()?.id!).subscribe({
      next: () => {
        alert('Pedido de adesão enviado com sucesso.');
      },
      error: (err) => {
        console.error(err);
        alert('Não foi possível enviar pedido de adesão.');
      },
    });
  }

  /**
   * Navega para a página de criação de um novo convite de partida.
   */
  protected sendMatchInvite(): void {
    this.router.navigate(['/team/createMatchInvite']);
  }

  /**
   * Elimina permanentemente a equipa atual.
   * Requer confirmação do utilizador.
   */
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