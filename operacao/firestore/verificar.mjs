// Confirma que os dados exportados foram importados: 3 salas e 15 mensagens.
const host = process.env.FIRESTORE_EMULATOR_HOST ?? '127.0.0.1:8085';
const projeto = process.env.FA_PROJETO ?? 'demo-futebol-amador';
const base = `http://${host}/v1/projects/${projeto}/databases/(default)/documents`;
const h = { Authorization: 'Bearer owner' };

const salas = (await (await fetch(`${base}/chatRooms`, { headers: h })).json()).documents ?? [];
let mensagens = 0;
for (const sala of salas) {
  const id = sala.name.split('/').pop();
  mensagens += ((await (await fetch(`${base}/chatRooms/${id}/messages`, { headers: h })).json()).documents ?? []).length;
}
console.log(`Importado: ${salas.length} salas, ${mensagens} mensagens.`);
if (salas.length !== 3 || mensagens !== 15) {
  console.error('FALHOU: os dados importados não coincidem com os exportados.');
  process.exit(1);
}
console.log('OK: exportação e importação coincidem.');
