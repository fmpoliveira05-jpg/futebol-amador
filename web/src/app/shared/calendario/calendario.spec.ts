import { chaveDia, feriadosNacionais, pascoa } from './feriados';
import { semanasDoMes, tituloMes } from './grelha';

describe('feriados', () => {
  it('calcula a Páscoa', () => {
    expect(chaveDia(pascoa(2024))).toBe('2024-03-31');
    expect(chaveDia(pascoa(2025))).toBe('2025-04-20');
    expect(chaveDia(pascoa(2026))).toBe('2026-04-05');
    expect(chaveDia(pascoa(2038))).toBe('2038-04-25');
  });

  it('tem os 13 feriados nacionais, com os móveis certos', () => {
    const f = feriadosNacionais(2026);
    expect(f.length).toBe(13);
    const porNome = Object.fromEntries(f.map((x) => [x.nome, x.data]));
    expect(porNome['Sexta-feira Santa']).toBe('2026-04-03');
    expect(porNome['Corpo de Deus']).toBe('2026-06-04');
    expect(porNome['Dia da Liberdade']).toBe('2026-04-25');
    expect(porNome['Natal']).toBe('2026-12-25');
    expect(f.map((x) => x.data)).toEqual([...f.map((x) => x.data)].sort());
  });
});

describe('grelha do mês', () => {
  it('começa à segunda e cobre o mês em semanas completas', () => {
    // Setembro de 2026 começa numa terça-feira e acaba numa quarta-feira.
    const semanas = semanasDoMes(2026, 8);
    expect(semanas.length).toBe(5);
    expect(semanas.every((s) => s.length === 7)).toBeTrue();
    expect(semanas[0][0].chave).toBe('2026-08-31');
    expect(semanas[0][1].chave).toBe('2026-09-01');
    expect(semanas[0][0].doMes).toBeFalse();
    expect(semanas[4][6].chave).toBe('2026-10-04');
    expect(semanas.flat().filter((d) => d.doMes).length).toBe(30);
  });

  it('um mês que começa à segunda não tem dias do mês anterior', () => {
    const semanas = semanasDoMes(2026, 5); // junho de 2026 começa numa segunda
    expect(semanas[0][0].chave).toBe('2026-06-01');
  });

  it('título em português', () => {
    expect(tituloMes(2026, 8)).toBe('Setembro de 2026');
  });
});
