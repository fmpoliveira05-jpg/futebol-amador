import { TeamDto } from '../Team/TeamDto';

/** Pedido de adiamento de um jogo (`GET /Team/{id}/PostPoneMatch`). */
export interface InfoPostPoneMatchDto {
  idMatch: string;
  /** Data marcada do jogo. */
  gameDate: string;
  /** Nova data proposta. */
  postPoneDate: string;
  team: TeamDto;
  opponent: TeamDto;
}

/** Resposta a um pedido de adiamento. */
export interface AcceptRefusePostPoneDto {
  idMatch: string;
  /** 0 = aceitar, 1 = rejeitar (`StatusPostPone` na API). */
  statusPostPone: 0 | 1;
  idTeam: string;
  idOpponent: string;
}
