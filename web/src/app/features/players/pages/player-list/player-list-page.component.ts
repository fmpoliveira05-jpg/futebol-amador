import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { PlayerService } from '../../services/player.service';
import { PlayerDetails } from '../../models/player.model';
import { POSITION_MAP } from '../../../../shared/constants/position-map';
import { PlayerListItem } from '../../models/player-list-item.model';

@Component({
  selector: 'app-player-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './player-list-page.component.html',
  styleUrl: './player-list-page.component.css',
})

export class PlayerListPageComponent {
  private readonly playerService = inject(PlayerService);
  private readonly router = inject(Router);

  protected readonly players = signal<PlayerListItem[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly searchTerm = signal<string>('');
  protected readonly POSITION_MAP = POSITION_MAP;
  protected readonly playersToShow = signal<number>(10);
  
  protected readonly visiblePlayers = computed(() =>
    this.filteredPlayers().slice(0, this.playersToShow())
  );

  protected readonly filteredPlayers = computed(() => {
    const term = this.searchTerm().toLowerCase().trim();
    const all = this.players().filter((p) => !p.haveTeam);

    if (!term) {
      return all;
    }

    return all.filter((p) =>
      p.name.toLowerCase().includes(term) ||
      POSITION_MAP[p.position].toLowerCase().includes(term)
    );
  });

  constructor() {
    this.loadPlayers();
  }

  protected onSearchTermChange(value: string): void {
    this.searchTerm.set(value);
  }

  protected openPlayer(player: PlayerDetails): void {
    this.router.navigate(['/players', player.playerId]);
  }

  private loadPlayers(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.playerService.getPlayers().subscribe({
      next: (players) => {
        this.players.set(players);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error(err);
        this.errorMessage.set('Não foi possível carregar a lista de jogadores.');
        this.isLoading.set(false);
      },
    });
  }

  protected loadMore(): void {
    this.playersToShow.update(v => v + 10);
  }

  protected openDetails(playerId: string): void {
    this.router.navigate(['/players/details', playerId]);
  }
}