import { MatchStatus } from './MatchStatus';

export class MatchDto {
    idMatch!: string;
    matchStatus!: MatchStatus;
    gameDate!: Date;
    matchResult!: number;
    isCompetitive!: boolean;
    team!: {
        idTeam: string;
        name: string;
        numGoals: number;
    };
    opponent!: {
        idTeam: string;
        name: string;
        numGoals: number;
    };
    pitchGame!: {
        name: string;
        address: string;
    };
    isHome!: boolean;
}