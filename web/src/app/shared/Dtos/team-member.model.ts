export interface TeamMember {
  playerId: string | null;
  name: string;
  email: string | null;
  phoneNumber: string | null;
  dateOfBirth: string;
  address: string | null;
  position: number;
  height: number;
  idTeam: string;
  teamName: string | null;
  isAdmin: boolean;
}