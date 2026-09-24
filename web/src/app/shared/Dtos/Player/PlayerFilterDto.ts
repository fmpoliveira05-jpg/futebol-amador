/** Filtros da lista de jogadores (aplicados no browser sobre a lista devolvida pela API). */
export class PlayerFilterDto {
  city = '';
  minAge = 16;
  maxAge = 80;
  minHeight = 100;
  maxHeight = 250;
  /** Posição (valor do enum `Position`) ou `null` para todas. */
  position: number | null = null;
  name = '';
}
