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

  /** Idade em anos (calculada pela API). */
  age?: number;

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
   * A altura do jogador, em centímetros.
   */
  height: number;

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
 * Dados enviados ao editar o perfil (`PUT /Player/update/{id}`). O id vai só no URL, e a API
 * confirma que é o do utilizador autenticado.
 */
export interface UpdatePlayerRequest {

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