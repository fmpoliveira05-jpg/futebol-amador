import { chaveDia } from './feriados';

/** Um dia da grelha mensal. */
export interface DiaGrelha {
  data: Date;
  chave: string;
  /** Pertence ao mês mostrado (os outros são os dias de enchimento das semanas). */
  doMes: boolean;
}

export const NOMES_MESES = [
  'janeiro', 'fevereiro', 'março', 'abril', 'maio', 'junho',
  'julho', 'agosto', 'setembro', 'outubro', 'novembro', 'dezembro',
];

export const DIAS_SEMANA = ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb', 'Dom'];

/**
 * Semanas (de segunda a domingo) que cobrem o mês `mes` (0 = janeiro) do ano `ano`.
 * Devolve sempre semanas completas: 4 a 6 linhas de 7 dias.
 */
export function semanasDoMes(ano: number, mes: number): DiaGrelha[][] {
  const primeiro = new Date(ano, mes, 1);
  const ultimo = new Date(ano, mes + 1, 0);
  const inicio = new Date(ano, mes, 1 - ((primeiro.getDay() + 6) % 7)); // segunda-feira
  const fim = new Date(ano, mes + 1, 0 + ((7 - ultimo.getDay()) % 7)); // domingo
  const semanas: DiaGrelha[][] = [];

  for (let d = inicio; d <= fim; d = new Date(d.getFullYear(), d.getMonth(), d.getDate() + 1)) {
    if (!semanas.length || semanas[semanas.length - 1].length === 7) {
      semanas.push([]);
    }
    semanas[semanas.length - 1].push({ data: d, chave: chaveDia(d), doMes: d.getMonth() === mes });
  }
  return semanas;
}

/** "setembro de 2026" com a primeira letra em maiúscula. */
export function tituloMes(ano: number, mes: number): string {
  const nome = NOMES_MESES[mes];
  return `${nome.charAt(0).toUpperCase()}${nome.slice(1)} de ${ano}`;
}
