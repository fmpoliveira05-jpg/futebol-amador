import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { MembershipRequestService } from '../../../services/membership-request.service';
import { MembershipRequest } from '../../..//shared/Dtos/membership-request.model';

/**
 * Componente responsável pela gestão de pedidos de adesão à equipa.
 * Permite carregar, aceitar, rejeitar e filtrar os pedidos de adesão enviados pelos jogadores.
 */
@Component({
  selector: 'app-team-membership-requests-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './team-membership-requests.component.html',
  styleUrls: ['./team-membership-requests.component.css'],
})
export class TeamMembershipRequestsPageComponent {
  private readonly membershipRequestService = inject(MembershipRequestService); // Serviço de pedidos de adesão

  protected readonly requests = signal<MembershipRequest[]>([]); // Lista de pedidos de adesão
  protected readonly isLoading = signal<boolean>(false); // Estado de carregamento
  protected readonly errorMessage = signal<string | null>(null); // Mensagem de erro
  protected readonly requestsToShow = signal<number>(10); // Número de pedidos a mostrar por vez

  // Filtra os pedidos de adesão enviados pelos jogadores
  protected readonly filteredRequests = computed(() =>
    this.requests().filter((r) => r.isPlayerSender)
  );

  // Define os pedidos visíveis de acordo com o número de pedidos a mostrar
  protected readonly visibleRequests = computed(() =>
    this.filteredRequests().slice(0, this.requestsToShow())
  );

  constructor() {
    this.loadRequests();
  }

  /**
   * Carrega os pedidos de adesão para a equipa atual.
   * Exibe uma mensagem de erro caso não seja possível carregar os pedidos.
   */
  private loadRequests(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.membershipRequestService
      .getMembershipRequestsForCurrentTeam()
      .subscribe({
        next: (requests) => {
          this.requests.set(requests);
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set(
            'Não foi possível carregar os pedidos de adesão.'
          );
          this.isLoading.set(false);
        },
      });
  }

  /**
   * Carrega mais pedidos de adesão, aumentando o número de pedidos visíveis.
   */
  protected loadMore(): void {
    this.requestsToShow.update((v) => v + 10);
  }

  /**
   * Aceita o pedido de adesão de um jogador à equipa.
   * Exibe uma confirmação antes de aceitar o pedido.
   */
  protected accept(request: MembershipRequest): void {
    if (
      !confirm(
        `Tens a certeza que queres aceitar o pedido de adesão do jogador "${request.player.name}"?`
      )
    ) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.membershipRequestService
      .acceptMembershipRequestTeam(request.requestId)
      .subscribe({
        next: () => {
          this.requests.update((list) =>
            list.filter((r) => r.requestId !== request.requestId)
          );
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Não foi possível aceitar o pedido de adesão.');
          this.isLoading.set(false);
        },
      });
  }

  /**
   * Rejeita o pedido de adesão de um jogador à equipa.
   * Exibe uma confirmação antes de rejeitar o pedido.
   */
  protected reject(request: MembershipRequest): void {
    if (
      !confirm(
        `Tens a certeza que queres rejeitar o pedido de adesão do jogador "${request.player.name}"?`
      )
    ) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.membershipRequestService
      .rejectMembershipRequestTeam(request.requestId)
      .subscribe({
        next: () => {
          this.requests.update((list) =>
            list.filter((r) => r.requestId !== request.requestId)
          );
          this.isLoading.set(false);
        },
        error: () => {
          this.errorMessage.set('Não foi possível rejeitar o pedido de adesão.');
          this.isLoading.set(false);
        },
      });
  }
}