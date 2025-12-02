/**
 * Interface que representa os detalhes completos de um jogador.
 * Utilizada para transferir informações detalhadas sobre o jogador, incluindo dados pessoais, dados da equipa e permissões.
 */
export interface PlayerDetails {

  /**
   * ID único do jogador.
   */
  playerId: string;

  /**
   * Nome completo do jogador.
   */
  name: string;

  /**
   * Endereço de e-mail do jogador.
   */
  email: string;

  /**
   * Número de telefone do jogador.
   */
  phoneNumber: string;

  /**
   * Data de nascimento do jogador (formato de string).
   */
  dateOfBirth: string;

  /**
   * Endereço residencial do jogador.
   */
  address: string;

  /**
   * Posição do jogador no campo, representada como um número (geralmente conforme um mapeamento de posições).
   */
  position: number;

  /**
   * Altura do jogador, em centímetros.
   */
  heigth: number;

  /**
   * A altura do jogador (opcional). Possivelmente um campo redundante com `heigth`.
   */
  height?: number;

  /**
   * A equipa do jogador (opcional).
   */
  team?: {
    idTeam: string;
    name: string;
  } | null;

  /**
   * Indica se o jogador tem permissões de administrador (opcional).
   */
  isAdmin?: boolean | null;
}

/**
 * Interface utilizada para a atualização dos dados de um jogador.
 * Contém os dados que podem ser alterados no perfil do jogador.
 */
export interface UpdatePlayerRequest {

  /**
   * ID único do jogador a ser atualizado.
   */
  playerId: string;

  /**
   * Nome atualizado do jogador.
   */
  Name: string;

  /**
   * Data de nascimento atualizada do jogador.
   */
  DateOfBirth: string;

  /**
   * Endereço atualizado do jogador.
   */
  Address: string;

  /**
   * E-mail atualizado do jogador.
   */
  Email: string;

  /**
   * Número de telefone atualizado do jogador.
   */
  Phone: string;

  /**
   * Posição atualizada do jogador (representada como um número).
   */
  Position: number;

  /**
   * Altura atualizada do jogador, em centímetros.
   */
  Height: number;
}