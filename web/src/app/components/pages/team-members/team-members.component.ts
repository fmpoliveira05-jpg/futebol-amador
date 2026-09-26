import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { TeamMembersService } from '../../../services/team-members.service';
import { TransferenciasService } from '../../../services/transferencias.service';
import { PlayerTeamDto } from '../../../shared/Dtos/Player/PlayerTeamDto';
import { POSITION_MAP } from '../../../shared/constants/position-map';
import { mensagemDeErro } from '../../../shared/http/erros';

/**
 * Componente responsável pela gestão dos membros de uma equipa.
 * Permite listar, filtrar, promover, despromover e remover jogadores da equipa.
 */
@Component({
  selector: 'app-team-members-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './team-members.component.html',
})
export class TeamMembersPageComponent {
  private readonly authService = inject(AuthService);
  private readonly teamMembersService = inject(TeamMembersService);
  private readonly transferencias = inject(TransferenciasService);
  protected readonly POSITION_MAP = POSITION_MAP;

  protected readonly members = signal<PlayerTeamDto[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly searchTerm = signal<string>('');
  protected readonly successMessage = signal<string | null>(null);
  /** Id do jogador com uma operação em curso (desativa os botões dessa linha). */
  protected readonly ocupado = signal<string | null>(null);

  /**
   * Computada: Filtra a lista de membros em tempo real com base no termo de pesquisa (nome ou posição).
   */
  protected readonly filteredMembers = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const list = this.members();

    if (!term) {
      return list;
    }

    return list.filter((m) => {
      const nameMatches = m.name?.toLowerCase().includes(term);
      const positionMatches = POSITION_MAP[m.position]?.toLowerCase().includes(term);
      return nameMatches || positionMatches;
    });
  });

  /**
   * Inicializa o componente obtendo o ID da equipa do utilizador logado.
   */
  ngOnInit(): void {
    this.getTeamIdOrShowError();
  }

  /**
   * Obtém o ID da equipa atual e carrega os membros, ou exibe erro se o utilizador não tiver equipa.
   */
  private getTeamIdOrShowError(): void {
    this.authService.getCurrentTeamId().subscribe((teamId) => {
      if (!teamId) {
        this.errorMessage.set('Não foi possível identificar a tua equipa.');
        this.isLoading.set(false);
      } else {
        this.loadMembers(teamId);
      }
    });
  }

  /**
   * Carrega a lista completa de membros da equipa via API.
   * @param teamId ID da equipa.
   */
  protected loadMembers(teamId: string): void {
    if (!teamId) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.teamMembersService.getTeamMembers(teamId).subscribe({
      next: (members) => {
        this.members.set(members);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Não foi possível carregar os membros da equipa.');
        this.isLoading.set(false);
      },
    });
  }

  /**
   * Atualiza o termo de pesquisa conforme o utilizador digita.
   * @param value O novo termo de pesquisa.
   */
  protected onSearchTermChange(value: string): void {
    this.searchTerm.set(value);
  }

  /** O utilizador é o administrador principal (quem criou a equipa). */
  protected readonly souPrincipal = computed(() =>
    this.members().some((m) => m.isCreator && m.playerId === this.authService.getCurrentPlayerId())
  );

  protected canPromote(member: PlayerTeamDto): boolean {
    return !member.isAdmin;
  }

  /** Só o administrador principal despromove, e ninguém o despromove a ele. */
  protected canDemote(member: PlayerTeamDto): boolean {
    return this.souPrincipal() && member.isAdmin && !member.isCreator;
  }

  /** Um administrador só pode ser removido pelo administrador principal. */
  protected canRemove(member: PlayerTeamDto): boolean {
    const eu = member.playerId === this.authService.getCurrentPlayerId();
    return !eu && !member.isCreator && (!member.isAdmin || this.souPrincipal());
  }

  /** Coloca o jogador no mercado de transferências ou retira-o. */
  protected alternarMercado(member: PlayerTeamDto): void {
    const teamId = member.team?.idTeam;
    if (!teamId) return;
    this.ocupado.set(member.playerId);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    const pedido = member.isListed
      ? this.transferencias.retirar(teamId, member.playerId)
      : this.transferencias.listar(teamId, member.playerId);
    pedido.subscribe({
      next: () => {
        this.ocupado.set(null);
        this.successMessage.set(
          member.isListed ? `${member.name} saiu do mercado.` : `${member.name} está no mercado de transferências.`
        );
        this.members.update((list) =>
          list.map((m) => (m.playerId === member.playerId ? { ...m, isListed: !member.isListed } : m))
        );
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorMessage.set(mensagemDeErro(e, 'Não foi possível atualizar o mercado.'));
      },
    });
  }

  /**
   * Promove um jogador a administrador da equipa.
   * @param member O jogador a promover.
   */
  protected promote(member: PlayerTeamDto): void {
    const teamId = member.team.idTeam;


    if (!teamId || !member.playerId) return;

    if (!confirm(`Queres promover "${member.name}" a administrador?`)) return;

    this.ocupado.set(member.playerId);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.teamMembersService.promoteMember(teamId, member.playerId).subscribe({
      next: () => {
        this.ocupado.set(null);
        this.successMessage.set(`${member.name} passou a administrador.`);
        this.members.update((list) =>
          list.map((m) =>
            m.playerId === member.playerId ? { ...m, isAdmin: true } : m
          )
        );
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorMessage.set(mensagemDeErro(e, 'Não foi possível promover o jogador.'));
      },
    });
  }

  /**
   * Remove o privilégio de administrador de um jogador.
   * @param member O jogador a despromover.
   */
  protected demote(member: PlayerTeamDto): void {
    const teamId = member.team.idTeam;

    if (!teamId || !member.playerId) return;

    if (!confirm(`Queres remover o estatuto de administrador de "${member.name}"?`)) return;

    this.ocupado.set(member.playerId);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.teamMembersService.demoteAdmin(teamId, member.playerId).subscribe({
      next: () => {
        this.ocupado.set(null);
        this.successMessage.set(`${member.name} deixou de ser administrador.`);
        this.members.update((list) =>
          list.map((m) =>
            m.playerId === member.playerId ? { ...m, isAdmin: false } : m
          )
        );
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorMessage.set(mensagemDeErro(e, 'Não foi possível retirar o estatuto de administrador.'));
      },
    });
  }

  /**
   * Remove (expulsa) um jogador da equipa.
   * @param member O jogador a remover.
   */
  protected remove(member: PlayerTeamDto): void {
    const teamId = member.team.idTeam;

    if (!teamId || !member.playerId) return;

    if (!confirm(`Queres expulsar o jogador "${member.name}" da equipa?`)) return;

    this.ocupado.set(member.playerId);
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.teamMembersService.removeMember(teamId, member.playerId).subscribe({
      next: () => {
        this.ocupado.set(null);
        this.successMessage.set(`${member.name} foi removido da equipa.`);
        this.members.update((list) =>
          list.filter((m) => m.playerId !== member.playerId)
        );
      },
      error: (e) => {
        this.ocupado.set(null);
        this.errorMessage.set(mensagemDeErro(e, 'Não foi possível expulsar o jogador.'));
      },
    });
  }
}