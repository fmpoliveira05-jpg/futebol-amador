/**
 * DTO que representa os filtros utilizados na pesquisa de jogadores.
 * Utilizado para transferir os critérios de filtro que serão aplicados à pesquisa de jogadores.
 */
export class PlayerFilterDto {
  
  /**
   * Cidade em que o jogador reside. Pode ser utilizado para filtrar os jogadores pela cidade.
   */
  city: string = '';

  /**
   * Idade mínima do jogador para a pesquisa. O valor padrão é 18.
   */
  minAge: number = 18;

  /**
   * Idade máxima do jogador para a pesquisa. O valor padrão é 70.
   */
  maxAge: number = 70;

  /**
   * Altura mínima do jogador para a pesquisa, em centímetros. O valor padrão é 100.
   */
  minHeight: number = 100;

  /**
   * Altura máxima do jogador para a pesquisa, em centímetros. O valor padrão é 250.
   */
  maxHeight: number = 250;

  /**
   * Posição do jogador para filtrar a pesquisa (ex: "Avançado", "Médio", "Defesa", etc.).
   */
  position: string = '';

  /**
   * Nome do jogador para pesquisa. Utilizado para filtrar jogadores pelo nome.
   */
  name: string = '';
}