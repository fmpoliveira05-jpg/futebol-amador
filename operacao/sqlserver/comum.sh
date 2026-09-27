#!/usr/bin/env bash
# Funções partilhadas pelos scripts de cópias de segurança do SQL Server (em Docker).
#
# Variáveis de ambiente:
#   FA_SQL_CONTENTOR   nome do contentor do SQL Server        (predefinido: futebol-sql)
#   FA_SQL_UTILIZADOR  utilizador SQL                         (predefinido: sa)
#   FA_SQL_PASSWORD    palavra-passe (obrigatória; nunca na linha de comandos)
#   FA_SQL_BD          base de dados                          (predefinido: FutebolAmador)
#   FA_BACKUP_DIR      pasta no anfitrião para as cópias      (predefinido: ./backups)
#   FA_BACKUP_RETENCAO_DIAS  dias a manter                    (predefinido: 14)
#   FA_BACKUP_COMPRESSAO     1 para WITH COMPRESSION (não existe na edição Express)
set -euo pipefail

CONTENTOR="${FA_SQL_CONTENTOR:-futebol-sql}"
UTILIZADOR="${FA_SQL_UTILIZADOR:-sa}"
BD="${FA_SQL_BD:-FutebolAmador}"
BACKUP_DIR="${FA_BACKUP_DIR:-./backups}"
RETENCAO_DIAS="${FA_BACKUP_RETENCAO_DIAS:-14}"
DIR_CONTENTOR="/var/opt/mssql/backup"

if [[ -z "${FA_SQL_PASSWORD:-}" ]]; then
  echo "Falta FA_SQL_PASSWORD." >&2
  exit 2
fi

# sqlcmd dentro do contentor. A palavra-passe vai por variável de ambiente (SQLCMDPASSWORD),
# não por argumento, para não aparecer na lista de processos.
sql() {
  docker exec -e SQLCMDPASSWORD="$FA_SQL_PASSWORD" "$CONTENTOR" \
    /opt/mssql-tools18/bin/sqlcmd -C -S localhost -U "$UTILIZADOR" -b -h -1 -W "$@"
}

# Nomes de identificadores só com letras, algarismos e _ (evita injeção nos comandos T-SQL).
validar_nome() {
  [[ "$1" =~ ^[A-Za-z0-9_]+$ ]] || { echo "Nome inválido: $1" >&2; exit 2; }
}

registo() { echo "[$(date -u +%FT%TZ)] $*"; }
