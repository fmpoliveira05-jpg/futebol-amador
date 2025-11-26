/**
 * DTO que representa os detalhes de um jogador.
 * Utilizado para transferir as informações completas de um jogador.
 */
export class PlayerDetailsDto {
  
  /**
   * ID único do jogador.
   */
  PlayerId!: string;

  /**
   * Nome completo do jogador.
   */
  Name!: string;

  /**
   * Endereço de e-mail do jogador.
   */
  Email!: string;

  /**
   * Número de telefone do jogador.
   */
  PhoneNumber!: string;

  /**
   * Data de nascimento do jogador (formato de string).
   */
  DateOfBirth!: string;

  /**
   * Endereço residencial do jogador.
   */
  Address!: string;

  /**
   * Posição do jogador no campo (ex: "Avançado", "Médio", "Defesa", etc.).
   */
  Position!: string;

  /**
   * Altura do jogador, em centímetros.
   */
  Height!: number;

  /**
   * ID da equipa à qual o jogador pertence (opcional).
   */
  IdTeam?: string;

  /**
   * Nome da equipa do jogador (opcional).
   */
  TeamName?: string;

  /**
   * Indica se o jogador é um administrador (opcional).
   */
  IsAdmin?: boolean;
}