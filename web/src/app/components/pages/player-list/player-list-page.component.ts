import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { PlayerService } from '../../../services/player.service';
import { PlayerDetails } from '../../../shared/Dtos/player.model';
import { POSITION_MAP_TONUMBER } from '../../../shared/constants/position-map-to-number';
import { PlayerListItem } from '../../../shared/Dtos/player-list-item.model';
import { FormsModule } from '@angular/forms';
import { PlayerFilterDto } from '../../../shared/Dtos/Player/PlayerFilterDto';
import { MembershipRequestService } from '../../../services/membership-request.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-player-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './player-list-page.component.html',
  styleUrls: ['./player-list-page.component.css'],
})
export class PlayerListPageComponent {
  private readonly playerService = inject(PlayerService);
  private readonly router = inject(Router);

  protected readonly players = signal<PlayerListItem[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  protected readonly filterDto = signal<PlayerFilterDto>(new PlayerFilterDto());
  protected readonly showFilters = signal<boolean>(false);

  protected readonly playersPerPage = 10;
  protected readonly currentPage = signal<number>(1);

  private readonly membershipRequestService = inject(MembershipRequestService);
  private readonly authService = inject(AuthService);

  protected readonly visiblePlayers = computed(() =>
    this.filteredPlayers().slice(
      (this.currentPage() - 1) * this.playersPerPage,
      this.currentPage() * this.playersPerPage
    )
  );

  protected readonly noResults = computed(() => {
    const { city, minAge, maxAge, minHeight, maxHeight, position, name } = this.filterDto();

    const isValidAge = minAge >= 18 && maxAge <= 70;
    const isValidHeight = minHeight >= 100 && maxHeight <= 250;

    return (
      this.filteredPlayers().length === 0 && 
      (city !== '' || name !== '' || position !== '' || 
      !isValidAge || !isValidHeight ||
      minHeight < 100 || maxHeight > 250 || 
      minAge < 18 || maxAge > 70)
    );
  });

  protected readonly filteredPlayers = computed(() => {
    const { city, minAge, maxAge, minHeight, maxHeight, position, name } = this.filterDto();

    const filtered = this.players().filter((p) => {
      const matchesCity = city ? p.address.toLowerCase().includes(city.toLowerCase()) : true;
      const matchesName = name ? p.name.toLowerCase().includes(name.toLowerCase()) : true;
      const matchesPosition = position ? p.position === POSITION_MAP_TONUMBER[position] : true;

      return (
        matchesCity &&
        matchesName &&
        matchesPosition &&
        p.age >= minAge &&
        p.age <= maxAge &&
        p.heigth >= minHeight &&
        p.heigth <= maxHeight
      );
    });

    return filtered;
  });

  constructor() {
  }

  protected openPlayer(player: PlayerDetails): void {
    this.router.navigate(['/players', player.playerId]);
  }

  protected loadPlayers(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.playerService.getPlayers().subscribe({
      next: (players) => {
        this.players.set(players);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set('Erro ao carregar jogadores');
        this.isLoading.set(false);
      },
    });
  }

  protected applyFilters(): void {
    this.currentPage.set(1);
    this.loadPlayers();
  }

  protected clearSearchResults(): void {
    this.players.set([]);
    this.currentPage.set(1);
  }

  protected loadMorePlayers(): void {
    this.currentPage.set(this.currentPage() + 1);
    this.loadPlayers();
  }

  protected goToPreviousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
      this.loadPlayers();
    }
  }

  protected hasMorePlayers(): boolean {
    return this.filteredPlayers().length > this.currentPage() * this.playersPerPage;
  }

  protected hasPreviousPage(): boolean {
    return this.currentPage() > 1;
  }

  protected sendMembershipRequest(playerId: string): void {
    this.membershipRequestService
      .sendMembershipRequestTeam(playerId)
      .subscribe({
        next: () => {
          this.successMessage.set('Pedido de adesão enviado com sucesso!');
        },
        error: (err) => {
          console.error(err);
          this.errorMessage.set('Não foi possível enviar o pedido de adesão.');
        },
      });
  }
}