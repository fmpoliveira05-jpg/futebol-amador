export class MatchDto {
    idMatch!: string;
    gameDate!: string;
    isCompetitive!: boolean;
    team!: {
        idTeam: string;
        name: string;
    };
    opponent!: {
        idTeam: string;
        name: string;
    };
    isHome!: boolean;
}