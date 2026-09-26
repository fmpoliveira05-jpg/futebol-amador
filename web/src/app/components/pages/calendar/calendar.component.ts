import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { CalendarService } from '../../../services/calendar.service';
import { AuthService } from '../../../services/auth.service';
import { CalendarDto } from '../../../shared/Dtos/Calendar/CalendarDto';
import { CalendarMarkerDto } from '../../../shared/Dtos/Competicao/competicao';
import { EstadoJogo, MATCH_STATUS } from '../../../shared/constants/match-status-map';
import { chaveDia, feriadosNacionais } from '../../../shared/calendario/feriados';
import { DIAS_SEMANA, semanasDoMes, tituloMes } from '../../../shared/calendario/grelha';
import { mensagemDeErro } from '../../../shared/http/erros';
import { LEGENDA, agruparPorDia, marcasDoDia, tipoDoJogo } from './calendario-dia';

/**
 * Ordem do calendário: primeiro os jogos por disputar (o mais próximo no topo), depois os
 * restantes do mais recente para o mais antigo.
 */
export function ordenarCalendario(jogos: CalendarDto[]): CalendarDto[] {
  const porJogar = (j: CalendarDto) => j.matchStatus === EstadoJogo.Agendado || j.matchStatus === EstadoJogo.Adiado;
  const tempo = (j: CalendarDto) => new Date(j.gameDate).getTime();
  return [...jogos].sort((a, b) => {
    if (porJogar(a) !== porJogar(b)) {
      return porJogar(a) ? -1 : 1;
    }
    return porJogar(a) ? tempo(a) - tempo(b) : tempo(b) - tempo(a);
  });
}

/**
 * Calendário da equipa em grelha mensal. Cada dia tem bolinhas coloridas (feriado, amigável, jogo
 * da liga, terminado, cancelado, adiado) e, ao escolher um dia, mostra os jogos com o resultado,
 * a hora, o campo e o motivo dos adiamentos e cancelamentos.
 */
@Component({
  selector: 'app-calendar',
  imports: [DatePipe, RouterLink],
  templateUrl: './calendar.component.html',
  styleUrls: ['./calendar.component.css'],
})
export class CalendarComponent implements OnInit {
  private readonly calendarService = inject(CalendarService);
  private readonly auth = inject(AuthService);

  /** Id da equipa (parâmetro da rota). */
  readonly idTeam = input.required<string>();

  protected readonly jogos = signal<CalendarDto[]>([]);
  protected readonly historico = signal<CalendarMarkerDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  private readonly hoje = new Date();
  protected readonly ano = signal(this.hoje.getFullYear());
  protected readonly mes = signal(this.hoje.getMonth());
  protected readonly diaEscolhido = signal<string>(chaveDia(this.hoje));

  protected readonly estados = MATCH_STATUS;
  protected readonly Estado = EstadoJogo;
  protected readonly legenda = LEGENDA;
  protected readonly diasSemana = DIAS_SEMANA;
  protected readonly tipoDoJogo = tipoDoJogo;
  protected readonly chaveHoje = chaveDia(this.hoje);

  protected readonly titulo = computed(() => tituloMes(this.ano(), this.mes()));
  protected readonly semanas = computed(() => semanasDoMes(this.ano(), this.mes()));

  /** Feriados do ano mostrado e dos vizinhos (as semanas podem passar para outro ano). */
  protected readonly feriados = computed(() => {
    const a = this.ano();
    return new Map([a - 1, a, a + 1].flatMap((x) => feriadosNacionais(x)).map((f) => [f.data, f.nome]));
  });

  protected readonly porDia = computed(() => agruparPorDia(this.jogos(), this.historico()));

  protected readonly conteudoEscolhido = computed(() => this.porDia().get(this.diaEscolhido()));
  protected readonly feriadoEscolhido = computed(() => this.feriados().get(this.diaEscolhido()) ?? null);
  protected readonly dataEscolhida = computed(() => {
    const [a, m, d] = this.diaEscolhido().split('-').map(Number);
    return new Date(a, m - 1, d);
  });

  /** Próximos jogos (para a lista por baixo da grelha). */
  protected readonly proximos = computed(() =>
    ordenarCalendario(this.jogos())
      .filter((j) => j.matchStatus === EstadoJogo.Agendado || j.matchStatus === EstadoJogo.Adiado)
      .slice(0, 5)
  );

  ngOnInit(): void {
    this.carregar();
  }

  protected get isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  protected carregar(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    forkJoin({
      jogos: this.calendarService.getMatchesForTeam(this.idTeam()),
      historico: this.calendarService.getHistorico(this.idTeam()).pipe(catchError(() => of([]))),
    }).subscribe({
      next: ({ jogos, historico }) => {
        this.jogos.set(jogos ?? []);
        this.historico.set(historico ?? []);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível carregar os jogos.'));
        this.isLoading.set(false);
      },
    });
  }

  protected mudarMes(delta: number): void {
    const d = new Date(this.ano(), this.mes() + delta, 1);
    this.ano.set(d.getFullYear());
    this.mes.set(d.getMonth());
  }

  protected irParaHoje(): void {
    this.ano.set(this.hoje.getFullYear());
    this.mes.set(this.hoje.getMonth());
    this.diaEscolhido.set(this.chaveHoje);
  }

  protected escolherDia(chave: string, doMes: boolean, data: Date): void {
    this.diaEscolhido.set(chave);
    if (!doMes) {
      this.ano.set(data.getFullYear());
      this.mes.set(data.getMonth());
    }
  }

  /** Mostra o mês e o dia de um jogo. */
  protected irPara(j: CalendarDto): void {
    const d = new Date(j.gameDate);
    this.escolherDia(chaveDia(d), false, d);
  }

  protected marcas(chave: string) {
    return marcasDoDia(this.porDia().get(chave), this.feriados().has(chave));
  }

  /** Equipa da casa primeiro ("B vs A" diz que se joga fora). */
  protected casa(j: CalendarDto) {
    return j.homeTeam ?? (j.isHome ? j.team : j.opponent);
  }

  protected fora(j: CalendarDto) {
    return j.awayTeam ?? (j.isHome ? j.opponent : j.team);
  }

  protected rotuloDia(chave: string, data: Date): string {
    const n = (this.porDia().get(chave)?.jogos.length ?? 0) + (this.porDia().get(chave)?.historico.length ?? 0);
    const feriado = this.feriados().get(chave);
    return [
      data.toLocaleDateString('pt-PT', { weekday: 'long', day: 'numeric', month: 'long' }),
      feriado ? `feriado: ${feriado}` : '',
      n ? `${n} ${n === 1 ? 'evento' : 'eventos'}` : '',
    ]
      .filter(Boolean)
      .join(', ');
  }

  protected podeAlterar(jogo: CalendarDto): boolean {
    return jogo.matchStatus === EstadoJogo.Agendado || jogo.matchStatus === EstadoJogo.Adiado;
  }
}
