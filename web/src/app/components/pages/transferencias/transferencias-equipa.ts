import { Component, computed, effect, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { TransferenciasService } from '../../../services/transferencias.service';
import {
  ESTADO_PROPOSTA,
  EstadoProposta,
  TeamTransferOffersDto,
  TransferOfferDto,
} from '../../../shared/Dtos/Competicao/competicao';
import { mensagemDeErro } from '../../../shared/http/erros';

type Separador = 'recebidas' | 'enviadas';

/**
 * Transferências da equipa (antes "pedidos de adesão"): propostas recebidas pelos nossos jogadores
 * (aceitar ou recusar) e propostas enviadas (retirar). Os pedidos de jogadores livres continuam na
 * página própria.
 */
@Component({
  selector: 'app-transferencias-equipa',
  imports: [DatePipe, RouterLink],
  templateUrl: './transferencias-equipa.html',
  styleUrl: './transferencias.css',
})
export class TransferenciasEquipa {
  private readonly servico = inject(TransferenciasService);
  protected readonly sessao = inject(AuthService).sessao;
  protected readonly estados = ESTADO_PROPOSTA;
  protected readonly EstadoProposta = EstadoProposta;

  protected readonly separador = signal<Separador>('recebidas');
  protected readonly dados = signal<TeamTransferOffersDto | null>(null);
  protected readonly erro = signal<string | null>(null);
  protected readonly aTratar = signal<string | null>(null);

  protected readonly lista = computed(() => {
    const d = this.dados();
    return d ? (this.separador() === 'recebidas' ? d.received : d.sent) : [];
  });

  protected readonly pendentesRecebidas = computed(
    () => this.dados()?.received.filter((o) => o.status === EstadoProposta.AEsperaDoClube).length ?? 0
  );

  constructor() {
    effect(() => {
      if (this.sessao().equipaId) {
        this.carregar();
      }
    });
  }

  private carregar(): void {
    this.servico.getPropostasEquipa(this.sessao().equipaId!).subscribe({
      next: (d) => this.dados.set(d),
      error: (e) => this.erro.set(mensagemDeErro(e, 'Não foi possível carregar as propostas.')),
    });
  }

  protected aceitar(o: TransferOfferDto): void {
    this.tratar(o, this.servico.aceitar(o.id));
  }

  protected recusar(o: TransferOfferDto): void {
    this.tratar(o, this.servico.recusar(o.id));
  }

  private tratar(o: TransferOfferDto, pedido: ReturnType<TransferenciasService['aceitar']>): void {
    this.aTratar.set(o.id);
    this.erro.set(null);
    pedido.subscribe({
      next: () => {
        this.aTratar.set(null);
        this.carregar();
      },
      error: (e) => {
        this.aTratar.set(null);
        this.erro.set(mensagemDeErro(e));
      },
    });
  }

  protected emAberto(o: TransferOfferDto): boolean {
    return o.status === EstadoProposta.AEsperaDoClube || o.status === EstadoProposta.AEsperaDoJogador;
  }
}
