import { MATCH_STATUS } from '../../constants/match-status-map';

/**
 * DTO para representar os jogos no calendário.
 */
export class CalendarDto {
  idMatch!: string;
  matchStatus!: number;
  matchResult!: string;
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
}