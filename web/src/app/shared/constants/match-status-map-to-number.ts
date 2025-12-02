/**
 * Mapeamento dos estados das partidas para valores numéricos.
 * Utilizado para converter o estado da partida em um valor numérico correspondente.
 */
export const MATCH_STATUS_MAP_TONUMBER: Record<string, number> = {
  'Scheduled': 0,
  'In Progress': 1,
  'Done': 2,
  'Post Poned': 3,
  'Cancelled': 4
};