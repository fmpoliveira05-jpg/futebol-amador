/**
 * Mapeamento dos valores numéricos para as posições de jogador.
 * Utilizado para converter um valor numérico em seu nome correspondente de posição.
 */
export const POSITION_MAP: Record<number, string> = {
  0: 'Avançado',
  1: 'Médio',
  2: 'Defesa',
  3: 'Guarda-redes',
};
/** Posições para listas de escolha (os valores seguem o enum `Position` da API). */
export const POSICOES: ReadonlyArray<{ label: string; value: number }> = Object.entries(POSITION_MAP).map(
  ([value, label]) => ({ label, value: Number(value) })
);
