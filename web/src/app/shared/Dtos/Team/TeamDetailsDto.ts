import { PitchDto } from "../Pitch/PitchDto";
import { PlayerDetailsDto } from "../Player/PlayerDetailsDto";

export class TeamDetailsDto{
    id!: string;
    name!: string;
    description?: string;
    foundationDate!: Date;
    totalPoints!: number;
    rankName!: string;
    pitchDto!: PitchDto;
    players!: PlayerDetailsDto[];
}