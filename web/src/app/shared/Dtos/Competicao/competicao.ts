/**
 * DTOs das ligas, transferências, onzes, relatório do jogo e perfil do jogador.
 * Os nomes e os valores dos enums seguem a API (ver docs/novas-funcionalidades.md).
 */

/** `SeasonStatus` da API. */
export enum EstadoEpoca {
  Inscricoes = 0,
  ADecorrer = 1,
  Terminada = 2,
}

export const ESTADO_EPOCA: Record<number, string> = {
  [EstadoEpoca.Inscricoes]: 'Inscrições abertas',
  [EstadoEpoca.ADecorrer]: 'A decorrer',
  [EstadoEpoca.Terminada]: 'Terminada',
};

export interface SeasonDto {
  id: string;
  leagueId: string;
  name: string;
  status: EstadoEpoca;
  startDate: string;
  endDate: string;
  teamCount: number;
}

export interface LeagueDto {
  id: string;
  name: string;
  level: number;
  promotionSpots: number;
  relegationSpots: number;
  seasonDurationDays: number;
  trophyName: string;
  teamCount: number;
  currentSeason: SeasonDto | null;
}

/** "V", "E" ou "D". */
export type ResultadoForma = 'V' | 'E' | 'D';

export interface StandingRowDto {
  position: number;
  teamId: string;
  teamName: string;
  icon: string | null;
  played: number;
  won: number;
  drawn: number;
  lost: number;
  goalsFor: number;
  goalsAgainst: number;
  goalDifference: number;
  points: number;
  form: ResultadoForma[];
  zone: 'PROMOTION' | 'RELEGATION' | null;
}

export interface StandingsDto {
  league: LeagueDto;
  season: SeasonDto | null;
  rows: StandingRowDto[];
}

export interface FixtureMatchDto {
  idMatch: string;
  date: string;
  status: number;
  homeTeamId: string;
  homeTeamName: string;
  awayTeamId: string;
  awayTeamName: string;
  homeGoals: number | null;
  awayGoals: number | null;
}

export interface FixtureRoundDto {
  round: number;
  matches: FixtureMatchDto[];
}

export interface TeamTitleDto {
  trophyName: string;
  leagueName: string;
  count: number;
  seasons: string[];
}

// ---------- Transferências ----------

/** `TransferOfferStatus` da API. */
export enum EstadoProposta {
  AEsperaDoClube = 0,
  AEsperaDoJogador = 1,
  Aceite = 2,
  Recusada = 3,
  Cancelada = 4,
}

export const ESTADO_PROPOSTA: Record<number, string> = {
  [EstadoProposta.AEsperaDoClube]: 'À espera do clube',
  [EstadoProposta.AEsperaDoJogador]: 'À espera do jogador',
  [EstadoProposta.Aceite]: 'Concluída',
  [EstadoProposta.Recusada]: 'Recusada',
  [EstadoProposta.Cancelada]: 'Cancelada',
};

export interface MarketFilter {
  hasTeam?: boolean | null;
  leagueId?: string | null;
  nationality?: string | null;
  position?: number | null;
  name?: string | null;
  onlyListed?: boolean | null;
}

export interface MarketPlayerDto {
  playerId: string;
  name: string;
  age: number;
  position: number;
  nationality: string | null;
  imageUrl: string | null;
  teamId: string | null;
  teamName: string | null;
  leagueId: string | null;
  leagueName: string | null;
  isListed: boolean;
}

export interface TransferOfferDto {
  id: string;
  playerId: string;
  playerName: string;
  fromTeamId: string;
  fromTeamName: string;
  toTeamId: string;
  toTeamName: string;
  status: EstadoProposta;
  message: string | null;
  viaListing: boolean;
  createdAt: string;
  decidedAt: string | null;
}

export interface TeamTransferOffersDto {
  received: TransferOfferDto[];
  sent: TransferOfferDto[];
}

// ---------- Perfil do jogador ----------

export const PE_PREFERIDO: Record<number, string> = { 0: 'Direito', 1: 'Esquerdo', 2: 'Ambos' };
export const SITUACAO_JOGADOR: Record<number, string> = { 0: 'Ativo', 1: 'Lesionado', 2: 'Indisponível' };

export interface PlayerTotalsDto {
  games: number;
  goals: number;
  assists: number;
  minutes: number;
  yellowCards: number;
  redCards: number;
}

export interface CareerLineDto extends PlayerTotalsDto {
  season: string;
  teamId: string;
  teamName: string;
}

export interface TransferHistoryDto {
  date: string;
  fromTeamName: string | null;
  toTeamName: string | null;
  kind: 'TRANSFERENCIA' | 'ADESAO' | 'SAIDA';
}

export interface PlayerProfileDto {
  id: string;
  name: string;
  imageUrl: string | null;
  dateOfBirth: string;
  age: number;
  position: number;
  height: number;
  weight: number | null;
  preferredFoot: number | null;
  status: number;
  nationality: string | null;
  countryOfBirth: string | null;
  currentTeam: { idTeam: string; name: string } | null;
  joinedTeamAt: string | null;
  isListed: boolean;
  totals: PlayerTotalsDto;
  career: CareerLineDto[];
  transfers: TransferHistoryDto[];
}

// ---------- Onze inicial ----------

export interface FormationSlotDto {
  slot: number;
  positionCode: string;
  /** Enum `Position`: 0 avançado, 1 médio, 2 defesa, 3 guarda-redes. */
  role: number;
  /** Percentagem da largura do campo. */
  x: number;
  /** Percentagem do comprimento; 100 é a baliza da própria equipa. */
  y: number;
}

export interface FormationDto {
  code: string;
  slots: FormationSlotDto[];
}

export interface LineupPlayerDto {
  slot: number | null;
  positionCode: string | null;
  playerId: string;
  playerName: string;
  position: number;
}

export interface LineupDto {
  matchId: string;
  teamId: string;
  formation: string | null;
  deadline: string;
  isLocked: boolean;
  isAutoFilled: boolean;
  exists: boolean;
  starters: LineupPlayerDto[];
  bench: LineupPlayerDto[];
}

export interface SaveLineupDto {
  formation: string;
  starters: { slot: number; playerId: string }[];
  bench: string[];
}

// ---------- Resultado e relatório ----------

export interface GoalEventDto {
  scorerId: string | null;
  assistId: string | null;
  minute: number | null;
}

export interface CardEventDto {
  playerId: string;
  /** 0 amarelo, 1 vermelho. */
  type: 0 | 1;
  minute: number | null;
}

export interface SubstitutionEventDto {
  playerOutId: string;
  playerInId: string;
  minute: number | null;
}

export interface MatchEventsDto {
  fouls: number | null;
  goals: GoalEventDto[];
  cards: CardEventDto[];
  substitutions: SubstitutionEventDto[];
}

export interface ResultMatchDto {
  idMatch: string;
  idTeam: string;
  numGoalsTeam: number;
  idOpponent: string;
  numGoalsOpponent: number;
  events: MatchEventsDto | null;
}

export interface FinishMatchStateDto {
  submitted: boolean;
  matchFinished: boolean;
  resultsCoincide: boolean | null;
  message: string;
}

export interface MatchEventViewDto {
  type: 'GOAL' | 'YELLOW_CARD' | 'RED_CARD' | 'SUBSTITUTION';
  minute: number | null;
  playerId: string | null;
  playerName: string | null;
  relatedPlayerId: string | null;
  relatedPlayerName: string | null;
}

export interface TeamReportDto {
  teamId: string;
  teamName: string;
  goals: number | null;
  fouls: number | null;
  yellowCards: number;
  redCards: number;
  substitutions: number;
  lineup: LineupDto | null;
  events: MatchEventViewDto[];
}

export interface MatchReportDto {
  matchId: string;
  date: string;
  status: number;
  isCompetitive: boolean;
  leagueName: string | null;
  round: number | null;
  pitchName: string | null;
  home: TeamReportDto;
  away: TeamReportDto;
}

// ---------- Calendário ----------

export interface CalendarMarkerDto {
  idMatch: string;
  date: string;
  kind: 'CANCELLED' | 'POSTPONED';
  reason: string | null;
  opponentName: string;
  newDate: string | null;
}
