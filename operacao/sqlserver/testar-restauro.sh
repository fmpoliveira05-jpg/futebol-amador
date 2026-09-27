#!/usr/bin/env bash
# Teste de ponta a ponta das cópias de segurança:
#   1. cria uma base de dados de teste com as migrações e dados (ferramenta Carga, "semear");
#   2. conta as linhas das tabelas principais;
#   3. faz a cópia (backup.sh), apaga a base de dados e restaura-a (restore.sh);
#   4. volta a contar e compara. Falha (código 1) se alguma contagem mudar.
#
#   FA_SQL_PASSWORD=... FA_SQL_CONTENTOR=fa-sql ./testar-restauro.sh
set -euo pipefail
AQUI="$(cd "$(dirname "$0")" && pwd)"
export FA_SQL_BD="${FA_SQL_BD_TESTE:-FaRestauroTeste}"
export FA_BACKUP_DIR="${FA_BACKUP_DIR:-$(mktemp -d)}"
source "$AQUI/comum.sh"
validar_nome "$BD"

TABELAS=(User Player Team Pitch Rank League Season SeasonTeam Match TeamStatistics Calendar Chat __EFMigrationsHistory)

contar() {
  local consulta="SET NOCOUNT ON;"
  for t in "${TABELAS[@]}"; do consulta+=" SELECT '${t}', COUNT(*) FROM [${t}];"; done
  sql -d "$BD" -Q "$consulta" | awk 'NF {print $1"="$2}'
}

sql -Q "IF DB_ID(N'${BD}') IS NOT NULL BEGIN ALTER DATABASE [${BD}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${BD}]; END"

registo "1. Migrações e dados de teste em ${BD}"
porta="${FA_SQL_PORTA:-1433}"
projeto="$AQUI/../../backend/Ferramentas/Carga"
dotnet build "$projeto" -c Release -v q --nologo -p:NoWarn=CS8602%3BCS8603%3BCS8604%3BCS8618%3BCS8619%3BCS8625 >/dev/null
dotnet run -c Release --no-build --project "$projeto" -- semear \
  "Server=localhost,${porta};Database=${BD};User Id=${UTILIZADOR};Password=${FA_SQL_PASSWORD};TrustServerCertificate=True" 40 10 100

antes="$(contar)"
echo "$antes"

registo "2. Cópia de segurança"
copia="$("$AQUI/backup.sh" | tail -n 1)"

registo "3. Apagar ${BD} e restaurar"
sql -Q "ALTER DATABASE [${BD}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${BD}];"
"$AQUI/restore.sh" "$copia" "$BD"

depois="$(contar)"
registo "4. Comparação"
if [[ "$antes" == "$depois" ]]; then
  echo "$depois"
  registo "OK: todas as contagens coincidem depois do restauro (${#TABELAS[@]} tabelas)."
  sql -Q "ALTER DATABASE [${BD}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [${BD}];"
else
  diff <(echo "$antes") <(echo "$depois") || true
  registo "FALHOU: as contagens mudaram."
  exit 1
fi
