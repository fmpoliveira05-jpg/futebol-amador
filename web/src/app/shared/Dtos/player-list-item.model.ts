/**
 * Interface que representa as informações resumidas de um jogador, utilizadas em listas ou visualizações gerais.
 * Esta interface é ideal para exibir uma visão compacta dos jogadores, com informações básicas.
 */
export interface PlayerListItem {

  /**
   * ID único do jogador.
   */
  id: string;

  /**
   * Nome completo do jogador.
   */
  name: string;

  /**
   * Posição do jogador (representada como um número, geralmente conforme um mapeamento definido).
   */
  position: number;

  /**
   * Altura do jogador em centímetros.
   */
  heigth: number;

  /**
   * Indica se o jogador está associado a uma equipa ou não.
   */
  haveTeam: boolean;

  /**
   * Idade do jogador.
   */
  age: number;

  /**
   * Nome da equipa à qual o jogador pertence (opcional).
   * Pode ser `null` ou `undefined` se o jogador não estiver associado a uma equipa.
   */
  teamName?: string | null;

  /**
   * Endereço residencial do jogador.
   */
  address: string;
}