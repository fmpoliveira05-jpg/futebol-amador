// Cria salas e mensagens de teste no emulador do Firestore (REST; "Bearer owner" ignora as regras).
const host = process.env.FIRESTORE_EMULATOR_HOST ?? '127.0.0.1:8085';
const projeto = process.env.FA_PROJETO ?? 'demo-futebol-amador';
const base = `http://${host}/v1/projects/${projeto}/databases/(default)/documents`;

async function criar(caminho, campos) {
  const r = await fetch(`${base}/${caminho}`, {
    method: 'PATCH',
    headers: { Authorization: 'Bearer owner', 'Content-Type': 'application/json' },
    body: JSON.stringify({ fields: campos }),
  });
  if (!r.ok) throw new Error(`${caminho}: ${r.status} ${await r.text()}`);
}

const s = (v) => ({ stringValue: v });
for (let i = 1; i <= 3; i++) {
  await criar(`chatRooms/sala${i}`, {
    name: s(`Sala ${i}`),
    createdBy: s('uid-a'),
    members: { arrayValue: { values: [s('uid-a'), s('uid-b')] } },
  });
  for (let m = 1; m <= 5; m++) {
    await criar(`chatRooms/sala${i}/messages/m${m}`, {
      text: s(`Mensagem ${m} da sala ${i}`),
      senderId: s(m % 2 ? 'uid-a' : 'uid-b'),
      timestamp: { timestampValue: new Date(Date.UTC(2026, 8, 27, 10, m)).toISOString() },
    });
  }
}
console.log('Semeado: 3 salas, 15 mensagens.');
