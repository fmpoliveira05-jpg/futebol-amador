#!/usr/bin/env bash
# Teste da exportação/importação do Firestore com o emulador (sem tocar no projeto real):
#   1. arranca o emulador, cria salas e mensagens e exporta ao sair (--export-on-exit);
#   2. arranca outro emulador a partir da exportação (--import) e confere os dados.
# Requisitos: firebase-tools e Java 11+.   Uso: ./testar-exportacao-emulador.sh
set -euo pipefail
AQUI="$(cd "$(dirname "$0")" && pwd)"
PASTA="${1:-$(mktemp -d)/exportacao}"
cd "$AQUI/../../firebase"
export FA_PROJETO=demo-futebol-amador

echo "1. Exportar para ${PASTA}"
firebase emulators:exec --only firestore --project "$FA_PROJETO" --export-on-exit "$PASTA" "node $AQUI/semear.mjs"
test -f "$PASTA/firebase-export-metadata.json"

echo "2. Importar de ${PASTA}"
firebase emulators:exec --only firestore --project "$FA_PROJETO" --import "$PASTA" "node $AQUI/verificar.mjs"
