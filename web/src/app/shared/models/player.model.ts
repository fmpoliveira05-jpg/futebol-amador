export interface PlayerDetails {
  playerId: string;
  name: string;
  email: string;
  phoneNumber: string;
  dateOfBirth: string;
  address: string;
  position: number;
  heigth: number;
  height?: number;
  haveTeam: boolean;
  idTeam?: string | null;
  teamName?: string | null;
  isAdmin?: boolean | null;
}

export interface UpdatePlayerRequest {
  dto: {
    playerId: string;
    Name: string;
    DateOfBirth: string;
    Address: string;
    Email: string;
    Phone: string;
    Position: number;
    Height: number;
  };
}