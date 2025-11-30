import { PlayerDto } from "./Player/PlayerDto";
import { TeamDto } from "./Team/TeamDto";

/**
 * Interface que representa um pedido de adesão de um jogador a uma equipa (ou vice-versa).
 * Utilizada para transferir as informações do pedido de adesão entre o cliente e o servidor.
 */
export interface MembershipRequest {

  /**
   * ID único do pedido de adesão.
   */
  requestId: string;

  /**
   * Nome do jogador que fez o pedido de adesão.
   */
  player: PlayerDto;

  /**
   * Equipa que fez o pedido de adesão.
   */
  team: TeamDto;

  /**
   * Data em que o pedido de adesão foi feito (formato de string).
   */
  requestDate: string;

  /**
   * Indica se o pedido de adesão foi feito pelo jogador (se o jogador é o remetente do pedido).
   */
  isPlayerSender: boolean;
}