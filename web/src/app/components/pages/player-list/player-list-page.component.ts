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

/**
 * Componente responsável pela gestão da lista de jogadores.
 * Permite visualizar, filtrar, carregar mais jogadores e enviar pedidos de adesão.
 */
@Component({
  selector: 'app-player-list-page',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './player-list-page.component.html',
  styleUrls: ['./player-list-page.component.css'],
})
export class PlayerListPageComponent {
  private readonly playerService = inject(PlayerService); // Serviço de jogadores
  private readonly router = inject(Router); // Serviço de navegação

  protected readonly players = signal<PlayerListItem[]>([]); // Lista de jogadores
  protected readonly isLoading = signal<boolean>(false); // Estado de carregamento
  protected readonly errorMessage = signal<string | null>(null); // Mensagem de erro
  protected readonly successMessage = signal<string | null>(null); // Mensagem de sucesso

  protected readonly filterDto = signal<PlayerFilterDto>(new PlayerFilterDto()); // Filtros aplicados
  protected readonly showFilters = signal<boolean>(false); // Estado de exibição dos filtros

  protected readonly playersPerPage = 10; // Número de jogadores por página
  protected readonly currentPage = signal<number>(1); // Página atual

  private readonly membershipRequestService = inject(MembershipRequestService); // Serviço de pedidos de adesão

  // Lista de jogadores visíveis com base nos filtros e na página atual
  protected readonly visiblePlayers = computed(() =>
    this.filteredPlayers().slice(
      (this.currentPage() - 1) * this.playersPerPage,
      this.currentPage() * this.playersPerPage
    )
  );

  // Verifica se há resultados de pesquisa
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

  // Aplica os filtros na lista de jogadores
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

  ngOnInit() {
    this.loadPlayers();
  }

  // Abre a página de detalhes do jogador
  protected openPlayer(player: PlayerDetails): void {
    this.router.navigate(['/players', player.playerId]);
  }

  // Carrega a lista de jogadores
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

  // Aplica os filtros e recarrega os jogadores
  protected applyFilters(): void {
    this.currentPage.set(1);
    this.loadPlayers();
  }

  // Limpa os resultados de pesquisa
  protected clearSearchResults(): void {
    this.players.set([]);
    this.currentPage.set(1);
  }

  // Carrega mais jogadores, incrementando a página atual
  protected loadMorePlayers(): void {
    this.currentPage.set(this.currentPage() + 1);
    this.loadPlayers();
  }

  // Vai para a página anterior
  protected goToPreviousPage(): void {
    if (this.currentPage() > 1) {
      this.currentPage.set(this.currentPage() - 1);
      this.loadPlayers();
    }
  }

  // Verifica se há mais jogadores para carregar
  protected hasMorePlayers(): boolean {
    return this.filteredPlayers().length > this.currentPage() * this.playersPerPage;
  }

  // Verifica se a página atual é maior que 1
  protected hasPreviousPage(): boolean {
    return this.currentPage() > 1;
  }

  // Envia o pedido de adesão para um jogador
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