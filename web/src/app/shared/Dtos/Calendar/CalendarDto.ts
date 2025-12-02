import { MatchStatus } from '../Match/MatchStatus';

/**
 * DTO para representar os jogos no calendário.
 */
export class CalendarDto {
  idMatch!: string;
  matchStatus!: MatchStatus;
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