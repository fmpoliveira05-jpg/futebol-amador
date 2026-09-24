import { dataIso, paraHttpParams } from './http-params';

describe('paraHttpParams', () => {
  it('ignora valores vazios, nulos e indefinidos', () => {
    const params = paraHttpParams({ a: '', b: null, c: undefined, d: 'x' });
    expect(params.keys()).toEqual(['d']);
  });

  it('mantém o zero e o false (são filtros válidos)', () => {
    const params = paraHttpParams({ posicao: 0, emCasa: false });
    expect(params.get('posicao')).toBe('0');
    expect(params.get('emCasa')).toBe('false');
  });

  it('renomeia as chaves pedidas', () => {
    const params = paraHttpParams({ isRanked: true }, { isRanked: 'IsRanqued' });
    expect(params.get('IsRanqued')).toBe('true');
    expect(params.has('isRanked')).toBeFalse();
  });

  it('envia as datas como yyyy-MM-dd', () => {
    const params = paraHttpParams({ minDate: new Date(2026, 0, 5, 23, 30) });
    expect(params.get('minDate')).toBe('2026-01-05');
  });

  it('aceita filtros indefinidos', () => {
    expect(paraHttpParams(undefined).keys()).toEqual([]);
  });
});

describe('dataIso', () => {
  it('usa o fuso horário local (não o de UTC)', () => {
    expect(dataIso(new Date(2026, 11, 31, 23, 59))).toBe('2026-12-31');
  });
});
