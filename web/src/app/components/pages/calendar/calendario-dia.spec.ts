import { CalendarDto } from '../../../shared/Dtos/Calendar/CalendarDto';
import { EstadoJogo } from '../../../shared/constants/match-status-map';
import { agruparPorDia, marcasDoDia, tipoDoJogo } from './calendario-dia';

describe('calendário por dia', () => {
  const jogo = (id: string, data: string, estado: EstadoJogo, liga = false) =>
    ({ idMatch: id, gameDate: data, matchStatus: estado, isCompetitive: liga }) as CalendarDto;

  it('dá a cor certa a cada jogo', () => {
    expect(tipoDoJogo(jogo('a', '2026-10-03T15:00:00', EstadoJogo.Agendado))).toBe('amigavel');
    expect(tipoDoJogo(jogo('b', '2026-10-03T15:00:00', EstadoJogo.Agendado, true))).toBe('liga');
    expect(tipoDoJogo(jogo('c', '2026-10-03T15:00:00', EstadoJogo.Terminado, true))).toBe('terminado');
    expect(tipoDoJogo(jogo('d', '2026-10-03T15:00:00', EstadoJogo.Cancelado))).toBe('cancelado');
    expect(tipoDoJogo(jogo('e', '2026-10-03T15:00:00', EstadoJogo.Adiado))).toBe('adiado');
  });

  it('junta jogos e histórico no dia certo, com as marcas pela ordem da legenda', () => {
    const dias = agruparPorDia(
      [jogo('a', '2026-10-05T20:00:00', EstadoJogo.Agendado, true), jogo('b', '2026-10-05T10:00:00', EstadoJogo.Terminado)],
      [{ idMatch: 'c', date: '2026-10-05T18:00:00', kind: 'POSTPONED', reason: 'Chuva', opponentName: 'X', newDate: null }]
    );
    const dia = dias.get('2026-10-05');
    expect(dia?.jogos.map((j) => j.idMatch)).toEqual(['b', 'a']);
    expect(marcasDoDia(dia, true)).toEqual(['feriado', 'liga', 'terminado', 'adiado']);
    expect(marcasDoDia(undefined, false)).toEqual([]);
  });
});
