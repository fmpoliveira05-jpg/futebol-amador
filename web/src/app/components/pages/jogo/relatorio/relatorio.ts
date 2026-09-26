import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../services/auth.service';
import { JogoService } from '../../../../services/jogo.service';
import { FormationDto, MatchReportDto, TeamReportDto } from '../../../../shared/Dtos/Competicao/competicao';
import { MATCH_STATUS } from '../../../../shared/constants/match-status-map';
import { mensagemDeErro } from '../../../../shared/http/erros';
import { Campo, JogadorNoCampo } from '../../../partials/campo/campo';

const ICONE: Record<string, string> = { GOAL: '⚽', YELLOW_CARD: '🟨', RED_CARD: '🟥', SUBSTITUTION: '↔' };

/** Relatório de um jogo: resultado, onzes, marcadores, cartões, substituições e faltas. */
@Component({
  selector: 'app-relatorio',
  imports: [Campo, DatePipe, RouterLink],
  templateUrl: './relatorio.html',
  styleUrl: './relatorio.css',
})
export class Relatorio {
  readonly matchId = input.required<string>();

  private readonly jogos = inject(JogoService);
  protected readonly sessao = inject(AuthService).sessao;
  protected readonly estados = MATCH_STATUS;
  protected readonly icone = ICONE;

  protected readonly relatorio = signal<MatchReportDto | null>(null);
  protected readonly taticas = signal<FormationDto[]>([]);
  protected readonly erro = signal<string | null>(null);

  protected readonly minhaEquipaJoga = computed(() => {
    const r = this.relatorio();
    const e = this.sessao().equipaId;
    return !!r && !!e && (r.home.teamId === e || r.away.teamId === e);
  });

  constructor() {
    this.jogos.taticas$.subscribe({ next: (t) => this.taticas.set(t), error: () => this.taticas.set([]) });
    effect(() => {
      this.jogos.getRelatorio(this.matchId()).subscribe({
        next: (r) => this.relatorio.set(r),
        error: (e) => this.erro.set(mensagemDeErro(e, 'Não foi possível carregar o jogo.')),
      });
    });
  }

  protected posicoes(equipa: TeamReportDto) {
    return this.taticas().find((t) => t.code === equipa.lineup?.formation)?.slots ?? [];
  }

  protected noCampo(equipa: TeamReportDto): Record<number, JogadorNoCampo | undefined> {
    const mapa: Record<number, JogadorNoCampo | undefined> = {};
    for (const s of equipa.lineup?.starters ?? []) {
      if (s.slot !== null) {
        mapa[s.slot] = { nome: s.playerName };
      }
    }
    return mapa;
  }

  protected descricao(tipo: string, jogador: string | null, relacionado: string | null): string {
    const j = jogador ?? 'Jogador não indicado';
    switch (tipo) {
      case 'GOAL':
        return relacionado ? `${j} (assistência de ${relacionado})` : j;
      case 'SUBSTITUTION':
        return `Sai ${j}, entra ${relacionado ?? '—'}`;
      default:
        return j;
    }
  }
}
