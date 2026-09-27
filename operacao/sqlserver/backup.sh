#!/usr/bin/env bash
# Cópia de segurança completa da base de dados, com data e hora no nome, verificação e retenção.
#
#   FA_SQL_PASSWORD=... ./backup.sh
#
# Resultado: $FA_BACKUP_DIR/<BD>_AAAAMMDD_HHMMSSZ.bak (+ .sha256). Apaga as cópias com mais de
# FA_BACKUP_RETENCAO_DIAS dias, no anfitrião e no contentor. Copiar a pasta para fora do servidor
# (outro disco, armazenamento de objetos) — ver docs/OPERACAO.md.
source "$(dirname "$0")/comum.sh"
validar_nome "$BD"

carimbo="$(date -u +%Y%m%d_%H%M%SZ)"
ficheiro="${BD}_${carimbo}.bak"
compressao=""
[[ "${FA_BACKUP_COMPRESSAO:-0}" == "1" ]] && compressao="COMPRESSION, "

mkdir -p "$BACKUP_DIR"
docker exec "$CONTENTOR" mkdir -p "$DIR_CONTENTOR"

registo "Cópia de ${BD} para ${ficheiro}"
sql -Q "BACKUP DATABASE [${BD}] TO DISK = N'${DIR_CONTENTOR}/${ficheiro}' WITH ${compressao}CHECKSUM, INIT, NAME = N'${BD} ${carimbo}', STATS = 25"

registo "Verificação (RESTORE VERIFYONLY)"
sql -Q "RESTORE VERIFYONLY FROM DISK = N'${DIR_CONTENTOR}/${ficheiro}' WITH CHECKSUM"

docker cp "${CONTENTOR}:${DIR_CONTENTOR}/${ficheiro}" "${BACKUP_DIR}/${ficheiro}"
( cd "$BACKUP_DIR" && sha256sum "$ficheiro" > "${ficheiro}.sha256" )
chmod 600 "${BACKUP_DIR}/${ficheiro}" "${BACKUP_DIR}/${ficheiro}.sha256"

registo "Retenção: apagar cópias com mais de ${RETENCAO_DIAS} dias"
find "$BACKUP_DIR" -maxdepth 1 -type f \( -name "${BD}_*.bak" -o -name "${BD}_*.bak.sha256" \) -mtime +"$RETENCAO_DIAS" -print -delete
docker exec "$CONTENTOR" find "$DIR_CONTENTOR" -maxdepth 1 -type f -name "${BD}_*.bak" -mtime +"$RETENCAO_DIAS" -delete
# No contentor só fica a última (a cópia a sério está no anfitrião).
docker exec "$CONTENTOR" find "$DIR_CONTENTOR" -maxdepth 1 -type f -name "${BD}_*.bak" ! -name "$ficheiro" -delete

registo "Concluído: ${BACKUP_DIR}/${ficheiro} ($(du -h "${BACKUP_DIR}/${ficheiro}" | cut -f1))"
echo "${BACKUP_DIR}/${ficheiro}"
