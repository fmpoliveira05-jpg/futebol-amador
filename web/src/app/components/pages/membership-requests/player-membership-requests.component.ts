import { CommonModule } from '@angular/common';
import { Component, computed, inject, signal } from '@angular/core';
import { MembershipRequestService } from '../../../services/membership-request.service';
import { MembershipRequest } from '../../..//shared/Dtos/membership-request.model';

@Component({
  selector: 'app-player-membership-requests-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './player-membership-requests.component.html',
  styleUrls: ['./player-membership-requests.component.css'],
})
export class PlayerMembershipRequestsPageComponent {
  private readonly membershipRequestService = inject(MembershipRequestService);

  protected readonly requests = signal<MembershipRequest[]>([]);
  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly requestsToShow = signal<number>(10);

  protected readonly filteredRequests = computed(() =>
    this.requests().filter((r) => !r.isPlayerSender)
  );

  protected readonly visibleRequests = computed(() =>
    this.filteredRequests().slice(0, this.requestsToShow())
  );

  constructor() {
    this.loadRequests();
  }

  private loadRequests(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.membershipRequestService
      .getMembershipRequestsForCurrentPlayer()
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

  protected loadMore(): void {
    this.requestsToShow.update((v) => v + 10);
  }

  protected accept(request: MembershipRequest): void {
    if (
      !confirm(
        `Tens a certeza que queres aceitar o pedido de adesão da equipa "${request.teamName}"?`
      )
    ) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.membershipRequestService
      .acceptMembershipRequestPlayer(request.requestId)
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

  protected reject(request: MembershipRequest): void {
    if (
      !confirm(
        `Tens a certeza que queres rejeitar o pedido de adesão da equipa "${request.teamName}"?`
      )
    ) {
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.membershipRequestService
      .rejectMembershipRequestPlayer(request.requestId)
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