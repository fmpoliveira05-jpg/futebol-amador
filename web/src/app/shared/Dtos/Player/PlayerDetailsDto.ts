export class PlayerDetailsDto{
    PlayerId!: string;
    Name!: string;
    Email!: string;
    PhoneNumber!: string;
    DateOfBirth!: string;
    Address!: string;
    Position!: string;
    Height!: number;
    IdTeam?: string;
    TeamName?: string;
    IsAdmin?: boolean;
}