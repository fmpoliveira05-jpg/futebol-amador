import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../../services/auth.service';
import { TeamMembersService } from '../../../services/team-members.service';
import { TeamMember } from '../../../shared/models/team-member.model';
import { POSITION_MAP } from '../../../shared/constants/position-map';

@Component({
  selector: 'app-team-members-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './team-members.component.html',
  styleUrl: './team-members.component.css',
})
export class TeamMembersPageComponent {
  private readonly authService = inject(AuthService);
  private readonly teamMembersService = inject(TeamMembersService);
  protected readonly POSITION_MAP = POSITION_MAP;

  protected readonly members = signal<TeamMember[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly searchTerm = signal<string>('');

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

  constructor() {}

  ngOnInit(): void {
    this.getTeamIdOrShowError();
  }

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

  protected onSearchTermChange(value: string): void {
    this.searchTerm.set(value);
  }

  protected canPromote(member: TeamMember): boolean {
    return !member.isAdmin;
  }

  protected canDemote(member: TeamMember): boolean {
    return member.isAdmin;
  }

  protected canRemove(member: TeamMember): boolean {
    return !member.isAdmin;
  }

  protected promote(member: TeamMember): void {
    const teamId = member.idTeam;

    if (!teamId || !member.playerId) return;

    if (!confirm(`Queres promover "${member.name}" a administrador?`)) return;

    this.teamMembersService.promoteMember(teamId, member.playerId).subscribe({
      next: () => {
        this.members.update((list) =>
          list.map((m) =>
            m.playerId === member.playerId ? { ...m, isAdmin: true } : m
          )
        );
      },
      error: () => {
        this.errorMessage.set('Não foi possível promover o jogador.');
      },
    });
  }

  protected demote(member: TeamMember): void {
    const teamId = member.idTeam;

    if (!teamId || !member.playerId) return;

    if (!confirm(`Queres remover o estatuto de administrador de "${member.name}"?`)) return;

    this.teamMembersService.demoteAdmin(teamId, member.playerId).subscribe({
      next: () => {
        this.members.update((list) =>
          list.map((m) =>
            m.playerId === member.playerId ? { ...m, isAdmin: false } : m
          )
        );
      },
      error: () => {
        this.errorMessage.set('Não foi possível rebaixar o jogador.');
      },
    });
  }

  protected remove(member: TeamMember): void {
    const teamId = member.idTeam;

    if (!teamId || !member.playerId) return;

    if (!confirm(`Queres expulsar o jogador "${member.name}" da equipa?`)) return;

    this.teamMembersService.removeMember(teamId, member.playerId).subscribe({
      next: () => {
        this.members.update((list) =>
          list.filter((m) => m.playerId !== member.playerId)
        );
      },
      error: () => {
        this.errorMessage.set('Não foi possível expulsar o jogador.');
      },
    });
  }
}