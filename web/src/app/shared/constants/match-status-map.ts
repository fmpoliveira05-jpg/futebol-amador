/**
 * Mapeamento dos valores numéricos para os estados das partidas.
 * Utilizado para converter um valor numérico em seu nome correspondente de estado de partida.
 */
export const MATCH_STATUS: Record<number, string> = {
   0: 'Scheduled',
   1: 'In Progress',
   2: 'Done',
   3: 'Post Poned',
   4: 'Cancelled'
};