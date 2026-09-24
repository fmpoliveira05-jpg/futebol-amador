/** Estados de um jogo, com os mesmos valores do enum `MatchStatus` da API. */
export enum EstadoJogo {
  Agendado = 0,
  EmCurso = 1,
  Terminado = 2,
  Adiado = 3,
  Cancelado = 4,
}

/** Texto de cada estado. */
export const MATCH_STATUS: Record<number, string> = {
  [EstadoJogo.Agendado]: 'Agendado',
  [EstadoJogo.EmCurso]: 'A decorrer',
  [EstadoJogo.Terminado]: 'Terminado',
  [EstadoJogo.Adiado]: 'Adiado',
  [EstadoJogo.Cancelado]: 'Cancelado',
};

/** Resultado de um jogo terminado, com os valores do enum `MatchResult` da API. */
export const MATCH_RESULT: Record<number, string> = {
  0: 'Vitória',
  1: 'Derrota',
  2: 'Empate',
  3: '',
};
