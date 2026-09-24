import { PitchDto } from '../Pitch/PitchDto';
import { PlayerDetails } from '../player.model';

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
  players: PlayerDetails[];
}
