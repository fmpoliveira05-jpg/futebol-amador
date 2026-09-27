# Operação em produção

Disponibilidade, limites, concorrência, desempenho, cópias de segurança e custos da API, da web e da
app Android. A segurança está em [SEGURANCA.md](SEGURANCA.md), os dados pessoais em [RGPD.md](RGPD.md)
e os custos em [CUSTOS.md](CUSTOS.md).

## Erros

| Situação | API | Web | Android |
|---|---|---|---|
| Rota inexistente | `404` ProblemDetails (com sessão; sem sessão `401`, para não revelar rotas) | página "Página não encontrada" (rota `**`) | — |
| Método errado | `405` ProblemDetails | — | — |
| Erro interno | `500` sem mensagem interna, com `traceId` para procurar nos logs | página `/erro` quando uma parte da app não carrega (versão nova) | mensagem genérica |
| Conflito (concorrência, duplicado) | `409` | mensagem da API | mensagem da API |
| Corpo demasiado grande | `413` | mensagem | mensagem |
| Serviço externo lento | `503` | mensagem | mensagem |

Todas as respostas de erro da API são `application/problem+json` em português (`Api/Middlewares/RespostasErro.cs`).
Cada página da web e cada ecrã Android que carrega dados tem os estados *a carregar*, *vazio* e *erro*
(web: blocos `@switch`/`estado`/`erro`; Android: `LoadingPage` e `UiState`).

## Tempos máximos e resiliência

| Onde | Valor | Chave |
|---|---|---|
| Kestrel: cabeçalhos | 15 s | `Limites:TimeoutCabecalhosSegundos` |
| Kestrel: corpo máximo | 1 MB | `Limites:TamanhoMaximoPedidoBytes` |
| Duração máxima de um pedido à API (fora dos hubs SignalR) | 30 s → 503 | `Limites:TimeoutPedidoSegundos` |
| Firebase REST, Turnstile, Cloudinary: por tentativa / total | 10 s / 25 s, disjuntor, 2 novas tentativas **só em métodos idempotentes** | `Limites:Externos:*` |
| Comandos SQL | 30 s, 3 novas tentativas em falhas transitórias | `BaseDados:CommandTimeoutSegundos`, `BaseDados:TentativasFalhaTransitoria` |
| Web (HttpClient) | 15 s por pedido; 1 nova tentativa só em `GET` com falha de rede | `TEMPOS_PEDIDOS` |
| Android (OkHttp) | ligação 10 s, leitura 20 s, escrita 20 s, total 30 s | `NetworkModule` |

Os POST ao Firebase (login, envio de e-mails) nunca são repetidos automaticamente.

## Pedidos duplicados

- **Clientes:** os botões ficam desativados enquanto o pedido está em curso (web: sinais `aGuardar`/`isLoading`;
  Android: `FormsViewModel.aSubmeter` e `enabled = !isLoading`).
- **`Idempotency-Key`** (`Api/Operacao/Idempotencia.cs`): um `POST` autenticado com a mesma chave devolve a
  resposta guardada (24 h, `Idempotencia:HorasRetencao`), com `Idempotent-Replayed: true`; a mesma chave em
  simultâneo → `409`; noutro pedido → `422`. A web envia-a ao criar equipas, convites de jogo, pedidos de adesão e
  propostas; a app Android em todos os `POST` (as repetições automáticas do OkHttp levam a mesma chave). A cache
  é em memória: com várias instâncias, usar Redis ou SQL Server (`IDistributedCache`).
- **Restrições únicas** (migração `ConcorrenciaEIndices`): e-mail da conta, nome da equipa, um pedido de adesão por
  jogador e equipa, um convite por equipas e hora, uma linha de estatísticas por equipa e jogo, uma proposta em aberto
  por jogador e equipa. A violação responde `409`.

A migração apaga pedidos de adesão e convites repetidos antes de criar as restrições. Nomes de equipas repetidos
fazem a migração falhar; verificar antes com:

```sql
SELECT Name, COUNT(*) FROM Team GROUP BY Name HAVING COUNT(*) > 1;
```

**Pagamentos:** a plataforma não tem pagamentos (as transferências são sem dinheiro), por isso não há
"pagamentos duplicados" a tratar.

## Concorrência

Coluna `rowversion` (`Versao`) em `Player`, `Team`, `Match`, `MatchInvite`, `TeamStatistics`, `PostPoneMatch`,
`TransferOffer` e `Season`. Dois pedidos que alteram a mesma linha → o segundo recebe `409` e nada é gravado (a
gravação é uma transação). Como `Player` usa TPT (tabelas `User` e `Player`), cada gravação de um jogador atualiza
`Player.AlteradoEm` para que o `rowversion` seja sempre verificado.

Testes com SQL Server real (`Tests/SqlServer`, categoria `SqlServer`, também no CI): alterações simultâneas ao mesmo
jogador, restrições únicas e **4 pedidos simultâneos a aceitar o mesmo convite → exatamente 1 jogo criado**.

## Consultas e índices

- Listas públicas paginadas no SQL (`/api/Team/listTeams`, `/api/Player/listPlayers`): `?page=&pageSize=`,
  50 por omissão, máximo 100. As listas de uma equipa ou de um jogador são paginadas na resposta
  (`X-Total-Count`).
- Consultas só de leitura sem tracking; perfil do jogador com `AsSplitQuery`; nomes dos jogadores por projeção.
- Índices novos: `User(Email)` único, `Team(Name)` único, `Team(CurrentPoints)`, `Match(MatchStatus, MatchDate)`,
  `Match(MatchDate)`, `Season(Status, StartDate)`, `MembershipRequests(IdPlayer, IdTeam)` único,
  `MatchInvite(IdSender, IdReceiver, GameDate)` único, `TeamStatistics(MatchesId, IdTeam)` único,
  `TransferOffer(PlayerId, IdToTeam)` único filtrado.
- Verificado num SQL Server 2022 (Docker) com 120 equipas e 2040 jogadores: número de comandos SQL por pedido
  constante (sem N+1) — ligas 2, classificação 6, lista de equipas 1, equipa 2, perfil do jogador 8; planos com
  *Index Seek* no login por e-mail, jogos por estado/data e pedidos de adesão, e *Index Scan* ordenado em
  `IX_Team_Name` na paginação.

## Cache

- API: Output Caching de 60 s (`Cache:PublicaSegundos`) em ligas, classificação, jornadas, táticas, lista de equipas e
  títulos (iguais para todos), invalidado por qualquer escrita com sucesso. Essas respostas levam
  `Cache-Control: public, max-age=60`; todas as outras `no-store`.
- Web: `shareReplay` na lista de ligas (5 min) e nas táticas.
- Android: cache HTTP do OkHttp (10 MB), que só guarda o que a API marca como público.

## Monitorização

- `GET /health/live` (processo) e `GET /health/ready` (base de dados e configuração do Firebase): anónimos, sem
  limitação de pedidos, JSON curto sem detalhes (`{"estado":"Healthy",...}`), `503` quando falham.
- GitHub Actions `uptime.yml`: a cada 15 minutos verifica a variável do repositório `FA_HEALTH_URL`
  (ex.: `https://api.exemplo.pt/health/ready`); sem a variável não faz nada.
- Monitor externo recomendado (avisa mais depressa e de fora do GitHub):
  - **UptimeRobot** (gratuito, 5 min): *New monitor* → HTTP(s) → URL de `/health/ready` → *Keyword* `Healthy` →
    alertas por e-mail/Telegram.
  - **Better Stack Uptime** (gratuito, 3 min): *Create monitor* → *URL becomes unavailable* ou *doesn't contain
    keyword* `Healthy` → política de escalonamento.
- Orquestrador (Docker/Kubernetes): *liveness* em `/health/live`, *readiness* em `/health/ready`.

## Teste de carga

Ferramenta em `backend/Ferramentas/Carga` (não faz parte da solução):

```bash
cd backend/Ferramentas/Carga
dotnet run -c Release -- semear "Server=localhost,1433;Database=FutebolAmadorCarga;User Id=sa;Password=...;TrustServerCertificate=True" 120 12 600
dotnet run -c Release -- carga http://127.0.0.1:5080 100 30 relatorio.json
```

Resultado (27/09/2026): API em Release (Kestrel, `ASPNETCORE_ENVIRONMENT=Production`) + SQL Server 2022 em Docker,
120 equipas, 2040 jogadores, 240 jogos; **100 utilizadores virtuais durante 30 s**, tudo na mesma máquina (2 CPU
partilhados, com o gerador de carga). Limite global de pedidos aumentado para o teste (todos vêm do mesmo IP).

| Endpoint | Pedidos | p50 (ms) | p95 (ms) | p99 (ms) | Erros |
|---|---:|---:|---:|---:|---:|
| GET /api/leagues | 6700 | 51 | 110 | 174 | 0 |
| GET /api/Leaderboard | 9059 | 51 | 105 | 188 | 0 |
| GET /api/leagues/{id}/standings | 8916 | 51 | 105 | 172 | 0 |
| GET /api/Team/listTeams | 6746 | 51 | 109 | 178 | 0 |
| GET /api/Team/{id} | 6597 | 91 | 231 | 377 | 0 |
| GET /api/Team/{id}/titles | 2177 | 52 | 120 | 205 | 0 |
| GET /api/lineups/formations | 2197 | 51 | 106 | 163 | 0 |
| GET /health/ready | 2243 | 71 | 174 | 284 | 0 |
| **Total** | **44 635** | **55** | **137** | **240** | **0** |

Débito: ~1490 pedidos/s. Antes da cache da lista de equipas e dos títulos: 570 pedidos/s, p95 345 ms.
Não medido: endpoints autenticados (precisam de tokens reais do Firebase) e hubs SignalR.

## Cópias de segurança

### SQL Server

Scripts em `operacao/sqlserver` (usam `sqlcmd` dentro do contentor; a palavra-passe vai por variável de ambiente):

```bash
export FA_SQL_PASSWORD='...' FA_SQL_CONTENTOR=futebol-sql FA_BACKUP_DIR=/srv/backups/futebol
./operacao/sqlserver/backup.sh                        # BD_AAAAMMDD_HHMMSSZ.bak + .sha256, verificação, retenção 14 dias
./operacao/sqlserver/restore.sh /srv/backups/futebol/FutebolAmador_....bak FutebolAmador_Verificacao
./operacao/sqlserver/testar-restauro.sh              # teste completo numa base de dados de teste
```

Agendar (cron, 03:15 UTC) e copiar para fora do servidor (outro disco ou armazenamento de objetos com
versões e ciclo de vida):

```cron
15 3 * * * FA_SQL_PASSWORD=... /opt/futebol/operacao/sqlserver/backup.sh >> /var/log/futebol-backup.log 2>&1
```

Teste de restauro executado (27/09/2026, SQL Server 2022 em Docker): migrações + dados de teste (500 jogadores,
40 equipas, 80 jogos), cópia com `CHECKSUM`/`VERIFYONLY`, `DROP DATABASE`, restauro, comparação das contagens de 13
tabelas (incluindo `__EFMigrationsHistory`): **todas iguais**.

Objetivos: RPO 24 h (cópia diária), RTO < 1 h. Testar o restauro uma vez por mês.

### Firestore (chat)

Exportações agendadas com o `gcloud` para um bucket com ciclo de vida:

```bash
gcloud storage buckets create gs://futebol-amador-backups --location=europe-west1 --uniform-bucket-level-access
gcloud storage buckets update gs://futebol-amador-backups --lifecycle-file=lifecycle.json   # apagar ao fim de 90 dias
gcloud firestore export gs://futebol-amador-backups/firestore/$(date -u +%F)
# restauro: gcloud firestore import gs://futebol-amador-backups/firestore/AAAA-MM-DD
```

`lifecycle.json`: `{"rule":[{"action":{"type":"Delete"},"condition":{"age":90}}]}`. Agendar com Cloud Scheduler
+ Cloud Functions/Workflows (ou um cron com uma conta de serviço com `roles/datastore.importExportAdmin`). Em
alternativa, a cópia de segurança agendada nativa do Firestore (`gcloud firestore backups schedules create
--database='(default)' --recurrence=daily --retention=14d`).

Teste com o emulador (sem tocar no projeto real): `operacao/firestore/testar-exportacao-emulador.sh` cria 3 salas e
15 mensagens, exporta (`--export-on-exit`), importa noutro emulador (`--import`) e confere — executado com sucesso
(3 salas, 15 mensagens).

## Fugas de informação

- `gitleaks git` em todo o histórico (784 commits): um único achado, o `JwtSettings:Secret` obsoleto do commit
  `2d910d5f` (ver SEGURANCA.md), registado em `.gitleaksignore`. `gitleaks dir` na árvore atual: nada.
  O workflow `segredos.yml` repete a verificação em cada push.
- `appsettings.json` sem ligações nem segredos (valores vazios).
- A API nunca devolve palavras-passe; os refresh tokens só vão no corpo para a app Android (a web recebe-os em
  cookies `HttpOnly`) — testes em `SegurancaIntegrationTests` e `CookiesSessaoIntegrationTests`.
- Logs sem e-mails, tokens nem conteúdo de notificações (revisto: `ILogger` com parâmetros estruturados; não há
  Serilog). Alguns logs dos hubs registam o uid do Firebase (pseudónimo), nunca o e-mail.

## Source maps

As configurações `production` e `demo` têm `sourceMap: false` (verificado: nenhum `.map` nem `sourceMappingURL` no
`dist`; o CI confirma). O nginx responde 404 a `*.map`. Android: a release é reduzida pelo R8 e o `mapping.txt` fica
em `app/build/outputs/mapping/release/`, fora do APK (verificado: 0 entradas no APK) — guardar esse ficheiro de cada
versão publicada para ler os relatórios de erros.
