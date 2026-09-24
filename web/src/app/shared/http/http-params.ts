import { HttpParams } from '@angular/common/http';

/**
 * Converte um objeto de filtros em parâmetros de query, ignorando valores vazios.
 * As datas são enviadas no formato `yyyy-MM-dd`, que é o que a API espera (`DateOnly`).
 *
 * @param filtros objeto com os filtros (pode ser `undefined`)
 * @param nomes renomeações de chaves (por exemplo, `{ isRanked: 'IsRanqued' }`)
 */
export function paraHttpParams(
  filtros?: object | null,
  nomes: Record<string, string> = {}
): HttpParams {
  let params = new HttpParams();
  if (!filtros) {
    return params;
  }
  for (const [chave, valor] of Object.entries(filtros)) {
    if (valor === null || valor === undefined || valor === '') {
      continue;
    }
    const nome = nomes[chave] ?? chave;
    const texto = valor instanceof Date ? dataIso(valor) : String(valor);
    params = params.set(nome, texto);
  }
  return params;
}

/** Data no formato `yyyy-MM-dd`, no fuso horário local (sem o desvio de `toISOString`). */
export function dataIso(data: Date): string {
  const mes = String(data.getMonth() + 1).padStart(2, '0');
  const dia = String(data.getDate()).padStart(2, '0');
  return `${data.getFullYear()}-${mes}-${dia}`;
}
