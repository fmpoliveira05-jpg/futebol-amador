import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AuthService } from '../../../../services/auth.service';
import { JogoService } from '../../../../services/jogo.service';
import { TeamMembersService } from '../../../../services/team-members.service';
import {
  FormationDto,
  FormationSlotDto,
  LineupDto,
  MatchReportDto,
} from '../../../../shared/Dtos/Competicao/competicao';
import { PlayerTeamDto } from '../../../../shared/Dtos/Player/PlayerTeamDto';
import { POSITION_MAP } from '../../../../shared/constants/position-map';
import { candidatosPosicao } from '../../../../shared/competicao/competicao';
import { mensagemDeErro } from '../../../../shared/http/erros';
import { Campo, JogadorNoCampo } from '../../../partials/campo/campo';

const MAX_SUPLENTES = 12;

/**
 * Onze inicial de um jogo: tática, titulares por posição e suplentes. O administrador define-o até
 * 2 horas antes do jogo; depois fica bloqueado e, se faltar, a API preenche-o automaticamente.
 * Mostra também o onze do adversário quando já é público.
 */
@Component({
  selector: 'app-onze',
  imports: [Campo, DatePipe, RouterLink],
  templateUrl: './onze.html',
  styleUrl: './onze.css',
})
export class Onze {
  readonly matchId = input.required<string>();

  private readonly jogos = inject(JogoService);
  private readonly membrosService = inject(TeamMembersService);
  protected readonly sessao = inject(AuthService).sessao;
  protected readonly posicaoNome = POSITION_MAP;

  protected readonly carregado = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly mensagem = signal<string | null>(null);
  protected readonly aGuardar = signal(false);

  protected readonly taticas = signal<FormationDto[]>([]);
  protected readonly relatorio = signal<MatchReportDto | null>(null);
  protected readonly plantel = signal<PlayerTeamDto[]>([]);
  protected readonly onzeAtual = signal<LineupDto | null>(null);
  protected readonly onzeAdversario = signal<LineupDto | null>(null);

  protected readonly tatica = signal<string>('4-3-3');
  /** Jogador escolhido para cada posição. */
  protected readonly titulares = signal<Record<number, string>>({});
  protected readonly suplentes = signal<string[]>([]);
  protected readonly posicaoAberta = signal<FormationSlotDto | null>(null);
  protected readonly mostrarTodos = signal(false);

  protected readonly posicoes = computed(() => this.taticas().find((t) => t.code === this.tatica())?.slots ?? []);
  protected readonly bloqueado = computed(() => this.onzeAtual()?.isLocked ?? true);
  protected readonly admin = computed(() => this.sessao().admin);

  protected readonly porId = computed(() => new Map(this.plantel().map((p) => [p.playerId, p])));

  protected readonly noCampo = computed(() => {
    const mapa: Record<number, JogadorNoCampo | undefined> = {};
    for (const [slot, id] of Object.entries(this.titulares())) {
      const p = this.porId().get(id);
      if (p) {
        mapa[Number(slot)] = { nome: p.name };
      }
    }
    return mapa;
  });

  protected readonly escolhidos = computed(
    () => new Set([...Object.values(this.titulares()), ...this.suplentes()])
  );

  protected readonly candidatos = computed(() => {
    const aberta = this.posicaoAberta();
    if (!aberta) {
      return [];
    }
    const atual = this.titulares()[aberta.slot];
    const outros = new Set([...this.escolhidos()].filter((id) => id !== atual));
    return candidatosPosicao(this.plantel(), aberta.role, outros, this.mostrarTodos());
  });

  protected readonly foraDoOnze = computed(() =>
    this.plantel()
      .filter((p) => !Object.values(this.titulares()).includes(p.playerId))
      .sort((a, b) => a.name.localeCompare(b.name, 'pt'))
  );

  protected readonly numTitulares = computed(() => Object.keys(this.titulares()).length);
  protected readonly titularesNecessarios = computed(() => Math.min(11, this.plantel().length));

  protected readonly adversario = computed(() => {
    const r = this.relatorio();
    const minha = this.sessao().equipaId;
    return r ? (r.home.teamId === minha ? r.away : r.home) : null;
  });

  protected readonly posicoesAdversario = computed(
    () => this.taticas().find((t) => t.code === this.onzeAdversario()?.formation)?.slots ?? []
  );

  protected readonly adversarioNoCampo = computed(() => {
    const mapa: Record<number, JogadorNoCampo | undefined> = {};
    for (const s of this.onzeAdversario()?.starters ?? []) {
      if (s.slot !== null) {
        mapa[s.slot] = { nome: s.playerName };
      }
    }
    return mapa;
  });

  constructor() {
    effect(() => {
      const matchId = this.matchId();
      const equipa = this.sessao().equipaId;
      if (!equipa) {
        return;
      }
      forkJoin({
        taticas: this.jogos.taticas$,
        relatorio: this.jogos.getRelatorio(matchId),
        plantel: this.membrosService.getTeamMembers(equipa),
        onze: this.jogos.getOnze(matchId, equipa),
      }).subscribe({
        next: ({ taticas, relatorio, plantel, onze }) => {
          this.taticas.set(taticas);
          this.relatorio.set(relatorio);
          this.plantel.set(plantel);
          this.aplicarOnze(onze);
          this.carregado.set(true);
          this.carregarAdversario();
        },
        error: (e) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível carregar o onze.'));
          this.carregado.set(true);
        },
      });
    });
  }

  private aplicarOnze(onze: LineupDto): void {
    this.onzeAtual.set(onze);
    if (onze.exists && onze.formation) {
      this.tatica.set(onze.formation);
      const t: Record<number, string> = {};
      for (const s of onze.starters) {
        if (s.slot !== null) {
          t[s.slot] = s.playerId;
        }
      }
      this.titulares.set(t);
      this.suplentes.set(onze.bench.map((b) => b.playerId));
    }
  }

  private carregarAdversario(): void {
    const adv = this.adversario();
    if (!adv) {
      return;
    }
    // Antes do prazo a API responde 403: o onze do adversário ainda é segredo.
    this.jogos.getOnze(this.matchId(), adv.teamId).subscribe({
      next: (o) => this.onzeAdversario.set(o.exists ? o : null),
      error: () => this.onzeAdversario.set(null),
    });
  }

  protected mudarTatica(codigo: string): void {
    const antiga = this.posicoes();
    this.tatica.set(codigo);
    // Mantém os jogadores nas posições com o mesmo índice (o guarda-redes fica sempre).
    const nova = this.posicoes();
    const t: Record<number, string> = {};
    for (const [slot, id] of Object.entries(this.titulares())) {
      if (nova.some((s) => s.slot === Number(slot)) && antiga.some((s) => s.slot === Number(slot))) {
        t[Number(slot)] = id;
      }
    }
    this.titulares.set(t);
    this.posicaoAberta.set(null);
  }

  protected abrir(p: FormationSlotDto): void {
    if (this.bloqueado() || !this.admin()) {
      return;
    }
    this.posicaoAberta.set(this.posicaoAberta()?.slot === p.slot ? null : p);
  }

  protected escolher(playerId: string): void {
    const aberta = this.posicaoAberta();
    if (!aberta) {
      return;
    }
    this.titulares.update((t) => ({ ...t, [aberta.slot]: playerId }));
    this.suplentes.update((s) => s.filter((id) => id !== playerId));
    this.posicaoAberta.set(null);
  }

  protected tirar(): void {
    const aberta = this.posicaoAberta();
    if (!aberta) {
      return;
    }
    this.titulares.update((t) => {
      const copia = { ...t };
      delete copia[aberta.slot];
      return copia;
    });
    this.posicaoAberta.set(null);
  }

  protected alternarSuplente(playerId: string): void {
    this.suplentes.update((s) =>
      s.includes(playerId) ? s.filter((id) => id !== playerId) : s.length < MAX_SUPLENTES ? [...s, playerId] : s
    );
  }

  protected guardar(): void {
    const equipa = this.sessao().equipaId;
    if (!equipa) {
      return;
    }
    this.aGuardar.set(true);
    this.erro.set(null);
    this.mensagem.set(null);
    this.jogos
      .guardarOnze(this.matchId(), equipa, {
        formation: this.tatica(),
        starters: Object.entries(this.titulares()).map(([slot, playerId]) => ({ slot: Number(slot), playerId })),
        bench: this.suplentes(),
      })
      .subscribe({
        next: (o) => {
          this.aplicarOnze(o);
          this.mensagem.set('Onze guardado.');
          this.aGuardar.set(false);
        },
        error: (e) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível guardar o onze.'));
          this.aGuardar.set(false);
        },
      });
  }

  protected nome(id: string): string {
    return this.porId().get(id)?.name ?? '—';
  }
}
