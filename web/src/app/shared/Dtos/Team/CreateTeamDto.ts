import { PitchDto } from "../Pitch/PitchDto";

export class CreateTeamDto{
    name!: string;
    description!: string;
    iconName!:string | null | undefined;
    iconPath?: string;
    homePitch!: PitchDto;
}