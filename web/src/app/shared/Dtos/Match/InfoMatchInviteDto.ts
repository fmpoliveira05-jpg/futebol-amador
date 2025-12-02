import { TeamDto } from "../Team/TeamDto";

export class InfoMatchInviteDto {
    id!: string;
    sender!: TeamDto;
    receiver!: TeamDto;
    gameDate!: Date;
    namePitch!: string;
}