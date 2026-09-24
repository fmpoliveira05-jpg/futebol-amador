import { Component, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PostponeMatchService } from '../../../../services/postpone-match.service';
import { InfoPostPoneMatchDto } from '../../../../shared/Dtos/PostPone/InfoPostPoneMatchDto';
import { mensagemDeErro } from '../../../../shared/http/erros';

/** Pedidos de adiamento que outras equipas fizeram à equipa do administrador. */
@Component({
  selector: 'app-postpone-requests',
  imports: [DatePipe, RouterLink],
  templateUrl: './postpone-requests.component.html',
})
export class PostponeRequestsComponent {
  private readonly service = inject(PostponeMatchService);

  /** Vem do parâmetro `:idTeam` da rota. */
  readonly idTeam = input.required<string>();

  protected readonly pedidos = signal<InfoPostPoneMatchDto[]>([]);
  protected readonly aCarregar = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly sucesso = signal<string | null>(null);
  protected readonly ocupado = signal<string | null>(null);

  constructor() {
    effect(() => this.carregar(this.idTeam()));
  }

  private carregar(idTeam: string): void {
    this.aCarregar.set(true);
    this.service.getPedidos(idTeam).subscribe({
      next: (lista) => {
        this.pedidos.set(lista);
        this.aCarregar.set(false);
      },
      error: (e) => {
        this.erro.set(mensagemDeErro(e, 'Não foi possível carregar os pedidos de adiamento.'));
        this.aCarregar.set(false);
      },
    });
  }

  /** A outra equipa do jogo (quem fez o pedido). */
  protected adversario(p: InfoPostPoneMatchDto): string {
    return p.team.idTeam === this.idTeam() ? p.opponent.name : p.team.name;
  }

  protected aceitar(p: InfoPostPoneMatchDto): void {
    this.responder(p, true);
  }

  protected rejeitar(p: InfoPostPoneMatchDto): void {
    this.responder(p, false);
  }

  private responder(p: InfoPostPoneMatchDto, aceitar: boolean): void {
    this.ocupado.set(p.idMatch);
    this.erro.set(null);
    this.sucesso.set(null);
    const pedido = aceitar ? this.service.aceitar(this.idTeam(), p) : this.service.rejeitar(this.idTeam(), p);

    pedido.subscribe({
      next: () => {
        this.ocupado.set(null);
        this.pedidos.update((lista) => lista.filter((x) => x.idMatch !== p.idMatch));
        this.sucesso.set(
          aceitar
            ? `Jogo contra ${this.adversario(p)} adiado.`
            : `Pedido de ${this.adversario(p)} rejeitado. O jogo mantém a data.`
        );
      },
      error: (e) => {
        this.ocupado.set(null);
        this.erro.set(mensagemDeErro(e, 'Não foi possível responder ao pedido.'));
      },
    });
  }
}
