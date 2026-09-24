/** Cria um JWT de teste (sem assinatura válida) que expira daqui a `segundos`. */
export function tokenQueExpiraEm(segundos: number): string {
  const b64 = (o: object) => btoa(JSON.stringify(o)).replace(/=+$/, '').replace(/\+/g, '-').replace(/\//g, '_');
  const exp = Math.floor(Date.now() / 1000) + segundos;
  return `${b64({ alg: 'none', typ: 'JWT' })}.${b64({ sub: 'jogador-1', exp })}.assinatura`;
}
