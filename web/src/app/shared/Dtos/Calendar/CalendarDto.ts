import { MATCH_STATUS } from '../../constants/match-status-map';

/**
 * DTO para representar os jogos no calendário.
 */
export class CalendarDto {
  idMatch!: string;
  matchStatus!: number;
  matchResult!: number;
  gameDate!: string;
  team!: {
    idTeam: string;
    name: string;
    numGoals: number;
  };
  opponent!: {
    idTeam: string;
    name: string;
    numGoals: number;
  };
  pitchGame!: {
    name: string;
    address: string;
  };
  isHome!: boolean;
  isCompetitive!: boolean;
  /** Equipa da casa e de fora (a API envia-as a partir das ligas). */
  homeTeam?: { idTeam: string; name: string; numGoals: number } | null;
  awayTeam?: { idTeam: string; name: string; numGoals: number } | null;
  leagueName?: string | null;
  round?: number | null;
  /** Motivo do cancelamento, do adiamento pendente ou do último adiamento aceite. */
  reason?: string | null;
  postponedFrom?: string | null;
}