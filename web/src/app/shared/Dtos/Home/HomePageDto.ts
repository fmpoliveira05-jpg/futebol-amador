/** Equipa resumida, como vem nos jogos da página inicial. */
export interface EquipaResumoDto {
  idTeam: string;
  name: string;
  imageUrl?: string | null;
}

/** Próximo jogo agendado. */
export interface ProximoJogoDto {
  idMatch: string;
  gameDate: string;
  isCompetitive: boolean;
  isHome: boolean;
  team: EquipaResumoDto;
  opponent: EquipaResumoDto;
}

/** Um dos últimos jogos terminados (o resultado vem como "3 - 1"). */
export interface JogoRecenteDto {
  opponent: EquipaResumoDto;
  result: string;
  matchResult: number;
}

/** Resposta de `GET /Team/homeTeam/{id}`. */
export interface HomePageDto {
  team: EquipaResumoDto;
  nextsMatchs?: ProximoJogoDto[] | null;
  historicPreviousGames?: JogoRecenteDto[] | null;
}
