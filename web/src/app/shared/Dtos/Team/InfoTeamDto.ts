import { InfoRankDto } from "../Rank/InfoRankDto";

export class InfoTeamDto{
    id!: string;
    name!: string;
    description!: string;
    address!: string;
    rank!: InfoRankDto;
    currentPoints!: number;
    averageAge!: number;
    playerCount!: number;
}