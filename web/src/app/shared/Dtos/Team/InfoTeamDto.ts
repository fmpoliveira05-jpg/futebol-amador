import { InfoRankDto } from '../Rank/InfoRankDto';

/** Uma equipa nas listas de pesquisa (`InfoTeamsDto` na API). */
export interface InfoTeamDto {
  id: string;
  name: string;
  description?: string | null;
  icon?: string | null;
  address: string;
  rank: InfoRankDto;
  currentPoints: number;
  averageAge: number;
  playerCount: number;
}
