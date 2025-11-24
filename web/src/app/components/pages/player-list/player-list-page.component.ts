import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { PlayerService } from '../../../services/player.service';
import { PlayerDetails } from '../../../shared/Dtos/player.model';
import { POSITION_MAP } from '../../../shared/constants/position-map';
import { PlayerListItem } from '../../../shared/Dtos/player-list-item.model';
import { FormsModule } from '@angular/forms';

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

  protected readonly cityFilter = signal<string>('');
  protected readonly minAge = signal<number>(0);
  protected readonly maxAge = signal<number>(100);
  protected readonly minHeight = signal<number>(0);
  protected readonly maxHeight = signal<number>(250);

  protected readonly showFilters = signal<boolean>(false);

  protected readonly playersToShow = signal<number>(10);

  protected readonly visiblePlayers = computed(() =>
    this.filteredPlayers().slice(0, this.playersToShow())
  );

  protected readonly filteredPlayers = computed(() => {
    const city = this.cityFilter().toLowerCase().trim();
    const all = this.players().filter((p) => !p.haveTeam);

    return all.filter((p) => {
      return (
        (city ? p.address.toLowerCase().includes(city) : true) &&
        p.age >= this.minAge() &&
        p.age <= this.maxAge() &&
        p.heigth >= this.minHeight() &&
        p.heigth <= this.maxHeight()
      );
    });
  });

  constructor() {
    this.loadPlayers();
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
}