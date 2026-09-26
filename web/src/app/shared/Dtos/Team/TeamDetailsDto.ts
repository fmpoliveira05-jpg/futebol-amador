import { PitchDto } from '../Pitch/PitchDto';
import { PlayerDetails } from '../player.model';
import { TeamTitleDto } from '../Competicao/competicao';

/** Resposta de `GET /Team/{id}`. */
export interface TeamDetailsDto {
  id: string;
  name: string;
  description?: string | null;
  /** URL, data URL ou base64 (ver `srcEmblema`). */
  icon?: string | null;
  foundationDate: string;
  totalPoints: number;
  rankName: string;
  pitchDto: PitchDto;
  players: (PlayerDetails & { isCreator?: boolean })[];
  /** Quem criou a equipa (o administrador principal). */
  creatorId?: string | null;
  leagueId?: string | null;
  leagueName?: string | null;
  /** Títulos agrupados por troféu. */
  titles?: TeamTitleDto[];
}
