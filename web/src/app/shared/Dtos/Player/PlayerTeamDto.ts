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
  /** É o administrador principal (quem criou a equipa). */
  isCreator?: boolean;
  /** 0 ativo, 1 lesionado, 2 indisponível. */
  status?: number;
  nationality?: string | null;
  /** Está no mercado de transferências. */
  isListed?: boolean;
  position!: number;
  team!: {
    idTeam: string;
    name: string;
  };
}