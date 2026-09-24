import { PitchDto } from '../Pitch/PitchDto';

export class CreateTeamDto {
  /** Preenchido pela API na resposta à criação. */
  id?: string;
  name!: string;
  description!: string;
  icon!: string | null | undefined;
  homePitch!: PitchDto;
}
