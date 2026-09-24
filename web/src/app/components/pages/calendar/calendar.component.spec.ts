import { ordenarCalendario } from './calendar.component';
import { CalendarDto } from '../../../shared/Dtos/Calendar/CalendarDto';
import { EstadoJogo } from '../../../shared/constants/match-status-map';

describe('ordenarCalendario', () => {
  const jogo = (id: string, data: string, estado: EstadoJogo) =>
    ({ idMatch: id, gameDate: data, matchStatus: estado }) as CalendarDto;

  it('mostra primeiro os jogos por disputar, do mais próximo para o mais distante', () => {
    const ordem = ordenarCalendario([
      jogo('passado-antigo', '2026-01-01T20:00:00Z', EstadoJogo.Terminado),
      jogo('futuro-longe', '2026-12-01T20:00:00Z', EstadoJogo.Agendado),
      jogo('passado-recente', '2026-03-01T20:00:00Z', EstadoJogo.Terminado),
      jogo('futuro-perto', '2026-10-01T20:00:00Z', EstadoJogo.Adiado),
      jogo('cancelado', '2026-11-01T20:00:00Z', EstadoJogo.Cancelado),
    ]).map((j) => j.idMatch);

    expect(ordem).toEqual(['futuro-perto', 'futuro-longe', 'cancelado', 'passado-recente', 'passado-antigo']);
  });

  it('não altera a lista original', () => {
    const lista = [jogo('b', '2026-02-01', EstadoJogo.Terminado), jogo('a', '2026-03-01', EstadoJogo.Terminado)];
    ordenarCalendario(lista);
    expect(lista.map((j) => j.idMatch)).toEqual(['b', 'a']);
  });
});
