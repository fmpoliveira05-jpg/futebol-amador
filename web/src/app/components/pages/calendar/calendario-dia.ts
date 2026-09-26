import { CalendarDto } from '../../../shared/Dtos/Calendar/CalendarDto';
import { CalendarMarkerDto } from '../../../shared/Dtos/Competicao/competicao';
import { EstadoJogo } from '../../../shared/constants/match-status-map';
import { chaveDia } from '../../../shared/calendario/feriados';

/** Tipos de marcação de um dia, pela ordem da legenda. */
export type TipoMarca = 'feriado' | 'amigavel' | 'liga' | 'terminado' | 'cancelado' | 'adiado';

export const LEGENDA: ReadonlyArray<{ tipo: TipoMarca; texto: string }> = [
  { tipo: 'feriado', texto: 'Feriado' },
  { tipo: 'amigavel', texto: 'Amigável marcado' },
  { tipo: 'liga', texto: 'Jogo da liga marcado' },
  { tipo: 'terminado', texto: 'Terminado' },
  { tipo: 'cancelado', texto: 'Cancelado' },
  { tipo: 'adiado', texto: 'Adiado' },
];

/** Cor (bolinha) de um jogo no calendário. */
export function tipoDoJogo(j: CalendarDto): TipoMarca {
  switch (j.matchStatus) {
    case EstadoJogo.Terminado:
      return 'terminado';
    case EstadoJogo.Cancelado:
      return 'cancelado';
    case EstadoJogo.Adiado:
      return 'adiado';
    default:
      return j.isCompetitive ? 'liga' : 'amigavel';
  }
}

/** O que aparece num dia: jogos nessa data e marcas do histórico (data original de adiamentos e cancelamentos). */
export interface ConteudoDia {
  jogos: CalendarDto[];
  historico: CalendarMarkerDto[];
}

/** Agrupa jogos e marcas por dia (AAAA-MM-DD, hora local). */
export function agruparPorDia(jogos: CalendarDto[], historico: CalendarMarkerDto[]): Map<string, ConteudoDia> {
  const mapa = new Map<string, ConteudoDia>();
  const obter = (chave: string) => {
    let c = mapa.get(chave);
    if (!c) {
      c = { jogos: [], historico: [] };
      mapa.set(chave, c);
    }
    return c;
  };
  for (const j of jogos) {
    obter(chaveDia(new Date(j.gameDate))).jogos.push(j);
  }
  for (const m of historico) {
    obter(chaveDia(new Date(m.date))).historico.push(m);
  }
  for (const c of mapa.values()) {
    c.jogos.sort((a, b) => new Date(a.gameDate).getTime() - new Date(b.gameDate).getTime());
  }
  return mapa;
}

/** Tipos de bolinha de um dia (sem repetições, pela ordem da legenda). */
export function marcasDoDia(conteudo: ConteudoDia | undefined, feriado: boolean): TipoMarca[] {
  const tipos = new Set<TipoMarca>();
  if (feriado) {
    tipos.add('feriado');
  }
  for (const j of conteudo?.jogos ?? []) {
    tipos.add(tipoDoJogo(j));
  }
  for (const m of conteudo?.historico ?? []) {
    tipos.add(m.kind === 'CANCELLED' ? 'cancelado' : 'adiado');
  }
  return LEGENDA.map((l) => l.tipo).filter((t) => tipos.has(t));
}
