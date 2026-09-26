import { candidatosPosicao, comSinal, textoZona } from './competicao';

describe('competição', () => {
  const plantel = [
    { playerId: 'a', name: 'Rui', position: 3 },
    { playerId: 'b', name: 'Ana', position: 1 },
    { playerId: 'c', name: 'Bia', position: 1 },
    { playerId: 'd', name: 'Carlos', position: 0 },
  ];

  it('mostra primeiro os jogadores da posição e tira os já escolhidos', () => {
    expect(candidatosPosicao(plantel, 1, new Set(), false).map((p) => p.playerId)).toEqual(['b', 'c']);
    expect(candidatosPosicao(plantel, 1, new Set(['b']), true).map((p) => p.playerId)).toEqual(['c', 'd', 'a']);
  });

  it('formata a diferença de golos e as zonas', () => {
    expect(comSinal(3)).toBe('+3');
    expect(comSinal(0)).toBe('0');
    expect(comSinal(-2)).toBe('−2');
    expect(textoZona('PROMOTION')).toBe('Subida');
    expect(textoZona('RELEGATION')).toBe('Descida');
    expect(textoZona(null)).toBeNull();
  });
});
