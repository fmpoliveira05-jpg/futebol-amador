import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { AuthService } from '../../../../services/auth.service';
import { JogoService } from '../../../../services/jogo.service';
import { TeamMembersService } from '../../../../services/team-members.service';
import {
  CardEventDto,
  FormationDto,
  FormationSlotDto,
  GoalEventDto,
  LineupDto,
  MatchReportDto,
  SubstitutionEventDto,
} from '../../../../shared/Dtos/Competicao/competicao';
import { PlayerTeamDto } from '../../../../shared/Dtos/Player/PlayerTeamDto';
import { mensagemDeErro } from '../../../../shared/http/erros';
import { Campo, JogadorNoCampo } from '../../../partials/campo/campo';

interface Opcao {
  id: string;
  nome: string;
}

/**
 * Formulário de fim de jogo (o mesmo que a app tem): resultado, faltas, marcadores e assistências,
 * cartões e substituições da equipa do administrador. O jogo só termina quando o resultado das duas
 * equipas coincide; os eventos de cada equipa ficam gravados nesse momento.
 */
@Component({
  selector: 'app-resultado',
  imports: [Campo, DatePipe, RouterLink],
  templateUrl: './resultado.html',
  styleUrl: './resultado.css',
})
export class Resultado {
  readonly matchId = input.required<string>();

  private readonly jogos = inject(JogoService);
  private readonly membros = inject(TeamMembersService);
  protected readonly sessao = inject(AuthService).sessao;

  protected readonly carregado = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly resposta = signal<{ texto: string; terminado: boolean } | null>(null);
  protected readonly aEnviar = signal(false);

  protected readonly relatorio = signal<MatchReportDto | null>(null);
  protected readonly onze = signal<LineupDto | null>(null);
  protected readonly plantel = signal<PlayerTeamDto[]>([]);
  protected readonly taticas = signal<FormationDto[]>([]);

  protected readonly golosNossos = signal(0);
  protected readonly golosDeles = signal(0);
  protected readonly faltas = signal<number | null>(null);
  protected readonly golos = signal<GoalEventDto[]>([]);
  protected readonly cartoes = signal<CardEventDto[]>([]);
  protected readonly substituicoes = signal<SubstitutionEventDto[]>([]);
  protected readonly selecionado = signal<FormationSlotDto | null>(null);

  protected readonly nossa = computed(() => {
    const r = this.relatorio();
    const minha = this.sessao().equipaId;
    return r ? (r.home.teamId === minha ? r.home : r.away) : null;
  });
  protected readonly deles = computed(() => {
    const r = this.relatorio();
    const n = this.nossa();
    return r && n ? (r.home === n ? r.away : r.home) : null;
  });

  protected readonly posicoes = computed(
    () => this.taticas().find((t) => t.code === this.onze()?.formation)?.slots ?? []
  );

  /** Titulares (do onze; sem onze, todos os membros). */
  protected readonly titulares = computed<Opcao[]>(() => {
    const o = this.onze();
    return o?.exists
      ? o.starters.map((s) => ({ id: s.playerId, nome: s.playerName }))
      : this.plantel().map((p) => ({ id: p.playerId, nome: p.name }));
  });

  protected readonly suplentes = computed<Opcao[]>(() => {
    const o = this.onze();
    return o?.exists ? o.bench.map((s) => ({ id: s.playerId, nome: s.playerName })) : this.plantel().map((p) => ({ id: p.playerId, nome: p.name }));
  });

  protected readonly todos = computed<Opcao[]>(() => {
    const vistos = new Map<string, Opcao>();
    for (const o of [...this.titulares(), ...this.suplentes()]) {
      vistos.set(o.id, o);
    }
    return [...vistos.values()];
  });

  protected readonly noCampo = computed(() => {
    const mapa: Record<number, JogadorNoCampo | undefined> = {};
    for (const s of this.onze()?.starters ?? []) {
      if (s.slot === null) {
        continue;
      }
      const golos = this.golos().filter((g) => g.scorerId === s.playerId).length;
      const amarelos = this.cartoes().filter((c) => c.playerId === s.playerId && c.type === 0).length;
      const vermelhos = this.cartoes().filter((c) => c.playerId === s.playerId && c.type === 1).length;
      const saiu = this.substituicoes().some((x) => x.playerOutId === s.playerId);
      const detalhe = [
        golos ? `⚽${golos > 1 ? '×' + golos : ''}` : '',
        amarelos ? '🟨'.repeat(amarelos) : '',
        vermelhos ? '🟥' : '',
        saiu ? '↓' : '',
      ].join('');
      mapa[s.slot] = { nome: s.playerName, detalhe: detalhe || undefined };
    }
    return mapa;
  });

  protected readonly totalAmarelos = computed(() => this.cartoes().filter((c) => c.type === 0).length);
  protected readonly totalVermelhos = computed(() => this.cartoes().filter((c) => c.type === 1).length);

  constructor() {
    effect(() => {
      const matchId = this.matchId();
      const equipa = this.sessao().equipaId;
      if (!equipa) {
        return;
      }
      forkJoin({
        relatorio: this.jogos.getRelatorio(matchId),
        onze: this.jogos.getOnze(matchId, equipa).pipe(catchError(() => of(null))),
        plantel: this.membros.getTeamMembers(equipa),
        taticas: this.jogos.taticas$,
      }).subscribe({
        next: ({ relatorio, onze, plantel, taticas }) => {
          this.relatorio.set(relatorio);
          this.onze.set(onze);
          this.plantel.set(plantel);
          this.taticas.set(taticas);
          this.carregado.set(true);
        },
        error: (e) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível carregar o jogo.'));
          this.carregado.set(true);
        },
      });
    });
  }

  // ---------- Ações rápidas a partir do campo ----------

  protected escolherNoCampo(p: FormationSlotDto): void {
    this.selecionado.set(this.selecionado()?.slot === p.slot ? null : p);
  }

  protected idSelecionado(): string | null {
    const slot = this.selecionado()?.slot;
    return this.onze()?.starters.find((s) => s.slot === slot)?.playerId ?? null;
  }

  protected golo(): void {
    this.golos.update((g) => [...g, { scorerId: this.idSelecionado(), assistId: null, minute: null }]);
    if (this.golos().length > this.golosNossos()) {
      this.golosNossos.set(this.golos().length);
    }
    this.selecionado.set(null);
  }

  protected cartao(type: 0 | 1): void {
    const id = this.idSelecionado();
    if (id) {
      this.cartoes.update((c) => [...c, { playerId: id, type, minute: null }]);
    }
    this.selecionado.set(null);
  }

  protected substituir(): void {
    const id = this.idSelecionado();
    if (id) {
      const entra = this.suplentes().find((s) => !this.substituicoes().some((x) => x.playerInId === s.id))?.id ?? '';
      this.substituicoes.update((s) => [...s, { playerOutId: id, playerInId: entra, minute: null }]);
    }
    this.selecionado.set(null);
  }

  // ---------- Edição das listas ----------

  protected numero(valor: string): number | null {
    const n = Number(valor);
    return valor === '' || Number.isNaN(n) ? null : n;
  }

  protected acrescentarGolo(): void {
    this.golos.update((g) => [...g, { scorerId: null, assistId: null, minute: null }]);
  }

  protected alterarGolo(i: number, campo: keyof GoalEventDto, valor: string): void {
    this.golos.update((g) =>
      g.map((x, j) => (j === i ? { ...x, [campo]: campo === 'minute' ? this.numero(valor) : valor || null } : x))
    );
  }

  protected acrescentarCartao(): void {
    const primeiro = this.todos()[0]?.id;
    if (primeiro) {
      this.cartoes.update((c) => [...c, { playerId: primeiro, type: 0, minute: null }]);
    }
  }

  protected alterarCartao(i: number, campo: keyof CardEventDto, valor: string): void {
    this.cartoes.update((c) =>
      c.map((x, j) =>
        j === i
          ? { ...x, [campo]: campo === 'minute' ? this.numero(valor) : campo === 'type' ? (Number(valor) as 0 | 1) : valor }
          : x
      )
    );
  }

  protected acrescentarSubstituicao(): void {
    this.substituicoes.update((s) => [...s, { playerOutId: this.titulares()[0]?.id ?? '', playerInId: this.suplentes()[0]?.id ?? '', minute: null }]);
  }

  protected alterarSubstituicao(i: number, campo: keyof SubstitutionEventDto, valor: string): void {
    this.substituicoes.update((s) =>
      s.map((x, j) => (j === i ? { ...x, [campo]: campo === 'minute' ? this.numero(valor) : valor } : x))
    );
  }

  protected remover(lista: 'golos' | 'cartoes' | 'substituicoes', i: number): void {
    if (lista === 'golos') {
      this.golos.update((x) => x.filter((_, j) => j !== i));
    } else if (lista === 'cartoes') {
      this.cartoes.update((x) => x.filter((_, j) => j !== i));
    } else {
      this.substituicoes.update((x) => x.filter((_, j) => j !== i));
    }
  }

  protected enviar(): void {
    const nossa = this.nossa();
    const deles = this.deles();
    if (!nossa || !deles) {
      return;
    }
    if (this.golos().length > this.golosNossos()) {
      this.erro.set(`Registaste ${this.golos().length} golos, mas a equipa marcou ${this.golosNossos()}.`);
      return;
    }
    this.aEnviar.set(true);
    this.erro.set(null);
    this.jogos
      .enviarResultado(this.matchId(), {
        idMatch: this.matchId(),
        idTeam: nossa.teamId,
        numGoalsTeam: this.golosNossos(),
        idOpponent: deles.teamId,
        numGoalsOpponent: this.golosDeles(),
        events: {
          fouls: this.faltas(),
          goals: this.golos(),
          cards: this.cartoes(),
          substitutions: this.substituicoes().filter((s) => s.playerInId && s.playerOutId),
        },
      })
      .subscribe({
        next: (r) => {
          this.resposta.set({ texto: r.message, terminado: r.matchFinished });
          this.aEnviar.set(false);
        },
        error: (e) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível registar o resultado.'));
          this.aEnviar.set(false);
        },
      });
  }
}
