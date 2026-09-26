/**
 * Feriados nacionais obrigatórios de Portugal (Código do Trabalho, art. 234.º), incluindo os
 * móveis: Sexta-feira Santa, Páscoa e Corpo de Deus. O Carnaval e os feriados municipais não entram.
 */
export interface Feriado {
  /** Data no formato AAAA-MM-DD (hora local). */
  data: string;
  nome: string;
}

/** Domingo de Páscoa (algoritmo de Meeus/Jones/Butcher, calendário gregoriano). */
export function pascoa(ano: number): Date {
  const a = ano % 19;
  const b = Math.floor(ano / 100);
  const c = ano % 100;
  const d = Math.floor(b / 4);
  const e = b % 4;
  const f = Math.floor((b + 8) / 25);
  const g = Math.floor((b - f + 1) / 3);
  const h = (19 * a + b - d - g + 15) % 30;
  const i = Math.floor(c / 4);
  const k = c % 4;
  const l = (32 + 2 * e + 2 * i - h - k) % 7;
  const m = Math.floor((a + 11 * h + 22 * l) / 451);
  const mes = Math.floor((h + l - 7 * m + 114) / 31);
  const dia = ((h + l - 7 * m + 114) % 31) + 1;
  return new Date(ano, mes - 1, dia);
}

/** Data local em AAAA-MM-DD. */
export function chaveDia(d: Date): string {
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}

function somarDias(d: Date, dias: number): Date {
  return new Date(d.getFullYear(), d.getMonth(), d.getDate() + dias);
}

/** Os 13 feriados nacionais de um ano, por ordem de data. */
export function feriadosNacionais(ano: number): Feriado[] {
  const p = pascoa(ano);
  const fixo = (mes: number, dia: number, nome: string): Feriado => ({ data: chaveDia(new Date(ano, mes - 1, dia)), nome });
  return [
    fixo(1, 1, 'Ano Novo'),
    { data: chaveDia(somarDias(p, -2)), nome: 'Sexta-feira Santa' },
    { data: chaveDia(p), nome: 'Páscoa' },
    fixo(4, 25, 'Dia da Liberdade'),
    fixo(5, 1, 'Dia do Trabalhador'),
    { data: chaveDia(somarDias(p, 60)), nome: 'Corpo de Deus' },
    fixo(6, 10, 'Dia de Portugal'),
    fixo(8, 15, 'Assunção de Nossa Senhora'),
    fixo(10, 5, 'Implantação da República'),
    fixo(11, 1, 'Dia de Todos os Santos'),
    fixo(12, 1, 'Restauração da Independência'),
    fixo(12, 8, 'Imaculada Conceição'),
    fixo(12, 25, 'Natal'),
  ].sort((x, y) => x.data.localeCompare(y.data));
}
