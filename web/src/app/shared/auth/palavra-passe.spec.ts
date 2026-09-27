import { cumprePoliticaPalavraPasse } from './palavra-passe';

describe('política de palavras-passe', () => {
  it('aceita palavras-passe fortes', () => {
    expect(cumprePoliticaPalavraPasse('Futebol#2026')).toBeTrue();
    expect(cumprePoliticaPalavraPasse('Árbitro-Ç 12')).toBeTrue();
  });

  it('recusa palavras-passe fracas ou longas demais', () => {
    for (const fraca of ['', 'Curta#1', 'semmaiusculas#1', 'SEMMINUSCULAS#1', 'SemAlgarismos#', 'SemSimbolos123']) {
      expect(cumprePoliticaPalavraPasse(fraca)).withContext(fraca).toBeFalse();
    }
    expect(cumprePoliticaPalavraPasse('Aa1#' + 'x'.repeat(125))).toBeFalse();
  });
});
