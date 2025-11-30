import { PitchDto } from '../Pitch/PitchDto';

export class CreateTeamDto {
  name!: string;
  description!: string;
  icon!: string | null | undefined;
  homePitch!: PitchDto;
}
