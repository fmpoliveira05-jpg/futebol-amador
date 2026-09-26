import { ResultadoForma } from '../Dtos/Competicao/competicao';

/** Ícone, cor e texto acessível de cada resultado da forma. */
export const FORMA: Record<ResultadoForma, { simbolo: string; titulo: string; classe: string }> = {
  V: { simbolo: '✓', titulo: 'Vitória', classe: 'forma--v' },
  E: { simbolo: '–', titulo: 'Empate', classe: 'forma--e' },
  D: { simbolo: '✕', titulo: 'Derrota', classe: 'forma--d' },
};

/** Texto da zona da classificação. */
export function textoZona(zona: string | null): string | null {
  return zona === 'PROMOTION' ? 'Subida' : zona === 'RELEGATION' ? 'Descida' : null;
}

/** Diferença de golos com sinal ("+3", "0", "−2"). */
export function comSinal(n: number): string {
  return n > 0 ? `+${n}` : n < 0 ? `−${Math.abs(n)}` : '0';
}

/**
 * Jogadores para uma posição do onze: primeiro os da posição (`role`), depois os outros
 * (se `todos`), sem os já escolhidos noutras posições. Ordena por nome.
 */
export function candidatosPosicao<T extends { playerId: string; name: string; position: number }>(
  plantel: T[],
  role: number,
  escolhidos: ReadonlySet<string>,
  todos: boolean
): T[] {
  const livres = plantel.filter((p) => !escolhidos.has(p.playerId));
  const daPosicao = livres.filter((p) => p.position === role).sort((a, b) => a.name.localeCompare(b.name, 'pt'));
  if (!todos) {
    return daPosicao;
  }
  const outros = livres.filter((p) => p.position !== role).sort((a, b) => a.name.localeCompare(b.name, 'pt'));
  return [...daPosicao, ...outros];
}

/** Minutos em texto ("1 234 min" → "1234 min" com separador de milhares português). */
export function minutos(n: number): string {
  return `${n.toLocaleString('pt-PT')} min`;
}
