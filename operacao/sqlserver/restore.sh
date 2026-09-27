#!/usr/bin/env bash
# Restaura uma cópia de segurança.
#
#   FA_SQL_PASSWORD=... ./restore.sh backups/FutebolAmador_20260927_030000Z.bak [BD_DESTINO]
#
# Sem BD_DESTINO restaura por cima de FA_SQL_BD — só com FA_RESTORE_SUBSTITUIR=1 (a base de dados
# atual é apagada). Para verificar uma cópia sem tocar na produção, indicar outro nome
# (por exemplo FutebolAmador_Verificacao) e apagá-la no fim.
source "$(dirname "$0")/comum.sh"

origem="${1:?Indica o ficheiro .bak}"
destino="${2:-$BD}"
validar_nome "$destino"
[[ -f "$origem" ]] || { echo "Não existe: $origem" >&2; exit 2; }

if [[ -f "${origem}.sha256" ]]; then
  ( cd "$(dirname "$origem")" && sha256sum -c "$(basename "$origem").sha256" )
fi

existe="$(sql -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = N'${destino}'" | tr -d '[:space:]')"
if [[ "$existe" != "0" && "${FA_RESTORE_SUBSTITUIR:-0}" != "1" ]]; then
  echo "A base de dados ${destino} já existe. Usar FA_RESTORE_SUBSTITUIR=1 para a substituir." >&2
  exit 3
fi

ficheiro="$(basename "$origem")"
docker exec "$CONTENTOR" mkdir -p "$DIR_CONTENTOR"
docker cp "$origem" "${CONTENTOR}:${DIR_CONTENTOR}/restauro_${ficheiro}"
# O docker cp deixa o ficheiro como root (e a cópia tem permissões 600): o SQL Server corre como mssql.
docker exec -u 0 "$CONTENTOR" chown mssql "${DIR_CONTENTOR}/restauro_${ficheiro}"

# Nomes lógicos dos ficheiros de dados e de log dentro da cópia.
lista="$(sql -Q "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = N'${DIR_CONTENTOR}/restauro_${ficheiro}'")"
mapfile -t logicos < <(echo "$lista" | awk 'NF {print $1}')
dados="${logicos[0]}"; log="${logicos[1]}"
validar_nome "$dados"; validar_nome "$log"

registo "Restaurar ${ficheiro} para ${destino}"
if [[ "$existe" != "0" ]]; then
  sql -Q "ALTER DATABASE [${destino}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE"
fi
sql -Q "RESTORE DATABASE [${destino}] FROM DISK = N'${DIR_CONTENTOR}/restauro_${ficheiro}' WITH CHECKSUM, REPLACE, RECOVERY,
  MOVE N'${dados}' TO N'/var/opt/mssql/data/${destino}.mdf',
  MOVE N'${log}' TO N'/var/opt/mssql/data/${destino}_log.ldf', STATS = 25"
sql -Q "ALTER DATABASE [${destino}] SET MULTI_USER"
docker exec "$CONTENTOR" rm -f "${DIR_CONTENTOR}/restauro_${ficheiro}"
registo "Restauro concluído: ${destino}"
