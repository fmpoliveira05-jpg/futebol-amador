export interface PlayerListItem {
  id: string;
  name: string;
  position: number;
  heigth: number;
  haveTeam: boolean;
  age: number;
  teamName?: string | null;
  address: string;
}