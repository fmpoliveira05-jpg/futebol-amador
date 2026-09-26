import { Component, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TransferenciasService } from '../../../services/transferencias.service';
import { TransferOfferDto } from '../../../shared/Dtos/Competicao/competicao';
import { mensagemDeErro } from '../../../shared/http/erros';

/** Propostas de transferência à espera da resposta do jogador (o clube já concordou). */
@Component({
  selector: 'app-transferencias-jogador',
  imports: [DatePipe, RouterLink],
  templateUrl: './transferencias-jogador.html',
  styleUrl: './transferencias.css',
})
export class TransferenciasJogador {
  private readonly servico = inject(TransferenciasService);

  protected readonly propostas = signal<TransferOfferDto[] | null>(null);
  protected readonly erro = signal<string | null>(null);
  protected readonly info = signal<string | null>(null);
  protected readonly aTratar = signal<string | null>(null);

  constructor() {
    this.carregar();
  }

  private carregar(): void {
    this.servico.getPropostasJogador().subscribe({
      next: (p) => this.propostas.set(p),
      error: (e) => this.erro.set(mensagemDeErro(e, 'Não foi possível carregar as propostas.')),
    });
  }

  protected responder(o: TransferOfferDto, aceitar: boolean): void {
    this.aTratar.set(o.id);
    this.erro.set(null);
    (aceitar ? this.servico.aceitar(o.id) : this.servico.recusar(o.id)).subscribe({
      next: () => {
        this.aTratar.set(null);
        this.info.set(
          aceitar
            ? `Bem-vindo à ${o.toTeamName}! Sai e volta a entrar para atualizar o menu.`
            : 'Proposta recusada.'
        );
        this.carregar();
      },
      error: (e) => {
        this.aTratar.set(null);
        this.erro.set(mensagemDeErro(e));
      },
    });
  }
}
