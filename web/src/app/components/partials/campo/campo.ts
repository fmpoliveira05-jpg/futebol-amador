import { Component, input, output } from '@angular/core';
import { FormationSlotDto } from '../../../shared/Dtos/Competicao/competicao';

/** Jogador colocado numa posição do campo. */
export interface JogadorNoCampo {
  nome: string;
  /** Texto pequeno por baixo do nome (por exemplo, cartões ou a posição). */
  detalhe?: string;
}

/**
 * Relvado desenhado em CSS, com um marcador por posição da tática. As coordenadas vêm da API
 * (`x` e `y` em percentagem, com a baliza da própria equipa em baixo). Os marcadores são botões
 * quando `selecionavel` é verdadeiro.
 */
@Component({
  selector: 'app-campo',
  templateUrl: './campo.html',
  styleUrl: './campo.css',
})
export class Campo {
  readonly posicoes = input.required<FormationSlotDto[]>();
  /** Jogador em cada posição (chave: `slot`). */
  readonly jogadores = input<Record<number, JogadorNoCampo | undefined>>({});
  readonly selecionavel = input(false);
  readonly selecionada = input<number | null>(null);
  readonly titulo = input('Campo');

  readonly escolher = output<FormationSlotDto>();

  protected iniciais(nome: string): string {
    const partes = nome.trim().split(/\s+/);
    return partes.length > 1 ? `${partes[0][0]}${partes[partes.length - 1][0]}` : nome.slice(0, 2);
  }

  protected primeiroNome(nome: string): string {
    const partes = nome.trim().split(/\s+/);
    return partes.length > 1 ? partes[partes.length - 1] : nome;
  }
}
