import { PitchDto } from "../Pitch/PitchDto";
import { PlayerDetails } from "../player.model";

export class TeamDetailsDto{
    id!: string;
    name!: string;
    description?: string;
    foundationDate!: Date;
    totalPoints!: number;
    rankName!: string;
    pitchDto!: PitchDto;
    players!: PlayerDetails[];
}