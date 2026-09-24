export class PlayerTeamDto {
  playerId!: string;
  name!: string;
  email!: string;
  phoneNumber!: string;
  dateOfBirth!: string;
  address!: string;
  age!: number;
  height!: number;
  isAdmin!: boolean;
  position!: number;
  team!: {
    idTeam: string;
    name: string;
  };
}