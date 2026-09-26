# Ligas, transferências, onze inicial e estatísticas

Este documento descreve as funcionalidades acrescentadas depois da revisão de 2026: o que fazem, as
decisões tomadas e o contrato da API usado pelo frontend web e pela app Android.

## Resumo

| Área | O que muda |
|---|---|
| Classificação | Por liga e por época, com PD, V, E, D, GM, GS, DG, P e a forma dos últimos 5 jogos. Vitória 3, empate 1, derrota 0. |
| Ligas e épocas | Ligas por escalões (substituem as divisões e o *matchmaking*). Calendário sorteado a duas voltas, subidas, descidas e troféus. |
| Jogos | Os jogos da liga são competitivos; os combinados entre equipas são amigáveis e não dão pontos. |
| Transferências | Os pedidos de adesão passam a "Transferências": mercado com filtros, jogadores listados e propostas entre equipas. |
| Perfil do jogador | Pé preferido, peso, situação, nacionalidade, país de nascimento, clube atual e "na equipa desde"; histórico por época, transferências, minutos e cartões. |
| Onze inicial | Tática, titulares por posição e suplentes, até 2 horas antes do jogo. Se faltar, é preenchido automaticamente. |
| Relatório do jogo | No fim do jogo cada administrador regista, da sua equipa, marcadores, assistências, cartões, substituições e faltas. |
| Calendário | Grelha mensal com cores por tipo de jogo, feriados nacionais e o motivo dos jogos adiados ou cancelados. |
| Administradores | Quem cria a equipa é o administrador principal: só ele pode despromover administradores. |

## Decisões

### D1. As ligas substituem as divisões e o *matchmaking*

Cada liga é um escalão (`Level` 1 é o mais alto). As quatro divisões iniciais passam a ser as quatro
ligas iniciais, com os mesmos nomes, e cada equipa fica na liga que corresponde à sua divisão. Os jogos
competitivos passam a ser só os da liga, marcados pelo sorteio; o *matchmaking* de domingo, o hub
`/MatchMaker` e o serviço em segundo plano que o alimentava foram retirados.

A tabela `Rank` e a coluna `Team.IdRank` ficam na base de dados, sem uso, para não obrigar a migrar dados
antigos de uma vez; `Team.CurrentPoints` deixa de ser atualizado (os pontos passam a ser calculados a
partir dos jogos da época).

### D2. A classificação calcula-se a partir dos jogos

Não se guardam pontos nem vitórias: a tabela de uma época calcula-se a partir dos jogos terminados da
época. Assim não há contadores dessincronizados quando um resultado é corrigido. Critérios de desempate,
por ordem: pontos, diferença de golos, golos marcados, nome.

### D3. Sorteio a duas voltas pelo método do círculo

Com *n* equipas (mais uma "folga" se *n* for ímpar) há 2(*n*−1) jornadas. A segunda volta é a primeira
com casa e fora trocados. O método do círculo alterna, para cada equipa, jogos em casa e fora, com no
máximo dois seguidos no mesmo sítio. A ordem das equipas é baralhada antes do sorteio.

As jornadas ficam espaçadas de forma a caberem na duração da liga (`SeasonDurationDays`, "a liga termina
ao fim de X dias"), com um mínimo de um dia. O jogo é no campo da equipa da casa, à hora de início
definida pelo super administrador.

### D4. Fecho da época

Uma época termina na data de fim (serviço em segundo plano) ou quando o super administrador a fecha. Os
jogos que ficaram por jogar não contam. Ao fechar:

1. o primeiro classificado recebe o troféu da liga (`TeamTitle`);
2. os `PromotionSpots` primeiros sobem para a liga do escalão acima e os `RelegationSpots` últimos descem;
3. cria-se a época seguinte de cada liga afetada em "Inscrições", já com as equipas certas.

### D5. Jogos da liga não se cancelam sem nova data

Um jogo da liga não pode ficar cancelado: cancelar exige uma nova data e o jogo é remarcado logo, ficando
o cancelamento (com o motivo e a data original) no histórico. Também se pode propor outra data por
acordo, pelo fluxo de adiamento (a outra equipa aceita ou recusa). Os adiamentos passam a ter motivo.

### D6. Transferências: clube e jogador têm de concordar

Não há dinheiro. Há dois caminhos:

- **Mercado:** o administrador lista um jogador da sua equipa; o administrador de outra equipa aceita
  (faz uma proposta a um jogador listado) e o clube vendedor já concordou.
- **Proposta:** o administrador de outra equipa propõe; o administrador da equipa do jogador aceita ou recusa.

Em ambos, o jogador tem a última palavra (ninguém muda de clube contra a vontade). Estados:
`PENDING_CLUB → PENDING_PLAYER → ACCEPTED`, ou `REJECTED`/`CANCELLED`. Um jogador que seja o único
administrador da equipa não pode ser transferido. Ao mudar de equipa perde o estatuto de administrador.

Os jogadores sem equipa continuam a entrar pelos pedidos de adesão e convites, que também ficam no
histórico de transferências ("Livre").

### D7. Onze inicial e preenchimento automático

O administrador define a tática (4-4-2, 4-3-3, 4-2-3-1, 3-5-2, 3-4-3, 5-3-2 ou 4-5-1), os 11 titulares e
até 12 suplentes, até **2 horas** antes do jogo. Cada posição mostra primeiro os jogadores dessa posição
(guarda-redes, defesas, médios ou avançados), mas aceita qualquer um. O adversário só vê o onze depois do
prazo.

Se o prazo passar sem onze, um serviço em segundo plano preenche-o: repete o último onze da equipa com os
jogadores que ainda lá estão e completa as posições vazias com jogadores dessa posição (depois com
quaisquer outros). Os administradores recebem uma notificação.

### D8. Eventos do jogo e minutos jogados

Cada administrador regista os eventos da **sua** equipa (é ele que sabe quem marcou): golos com marcador
e assistência opcionais, cartões com minuto, substituições (sai, entra, minuto) e o número de faltas. Os
golos com marcador não podem ser mais do que os golos da equipa. Os eventos só ficam gravados quando os
dois resultados coincidem.

Minutos jogados: um titular joga 90 minutos, menos os que faltarem quando sai ou é expulso; um suplente
que entra joga desde o minuto da entrada. Um jogo conta para o jogador se ele foi titular ou entrou.

### D9. Administrador principal

`Team.CreatorId` guarda quem criou a equipa. Só ele pode despromover administradores e ninguém o pode
despromover. Se sair da equipa, o estatuto passa para o administrador mais antigo (ou, sem outros
administradores, para o membro mais antigo, que é promovido). Nas equipas que já existiam, o
administrador principal passa a ser o administrador mais antigo.

### D10. Perfil do jogador

"Contrato" é mostrado como "Na equipa desde" (não há contratos com prazo num contexto amador). "Época"
é a da liga quando o jogo é de uma liga; os amigáveis contam na época desportiva da data do jogo (de
agosto a julho, por exemplo "2026/27").

### D11. Feriados

Os 13 feriados nacionais obrigatórios são calculados no cliente, incluindo os móveis (Sexta-feira Santa,
Páscoa e Corpo de Deus, a partir da data da Páscoa). O Carnaval e os feriados municipais não entram.

## Contrato da API

Os enums são serializados como inteiros, como no resto da API. As datas são ISO 8601 em UTC.

### Enums novos

| Enum | Valores |
|---|---|
| `PreferredFoot` | `RIGHT=0`, `LEFT=1`, `BOTH=2` |
| `PlayerStatus` | `ACTIVE=0` (Ativo), `INJURED=1` (Lesionado), `UNAVAILABLE=2` (Indisponível) |
| `SeasonStatus` | `REGISTRATION=0` (Inscrições), `IN_PROGRESS=1` (A decorrer), `FINISHED=2` (Terminada) |
| `TransferOfferStatus` | `PENDING_CLUB=0`, `PENDING_PLAYER=1`, `ACCEPTED=2`, `REJECTED=3`, `CANCELLED=4` |
| `CardType` | `YELLOW=0`, `RED=1` |

### Ligas e classificação

| Método | Rota | Quem | Resposta |
|---|---|---|---|
| GET | `/api/leagues` | público | `LeagueDto[]` |
| GET | `/api/leagues/{id}/standings?seasonId=` | público | `StandingsDto` (época atual por omissão) |
| GET | `/api/Leaderboard?leagueId=` | público | `StandingsDto` (liga de escalão 1 por omissão) |
| GET | `/api/leagues/seasons/{seasonId}/fixtures` | público | `FixtureRoundDto[]` |
| POST | `/api/leagues` | super admin | `LeagueDto` (corpo `CreateLeagueDto`) |
| POST | `/api/leagues/{id}/seasons` | super admin | `SeasonDto` (corpo `CreateSeasonDto`) |
| POST | `/api/leagues/seasons/{seasonId}/start` | super admin | `SeasonDto` (corpo `StartSeasonDto`) |
| POST | `/api/leagues/seasons/{seasonId}/close` | super admin | `SeasonDto` |
| POST | `/api/leagues/{id}/register/{teamId}` | admin da equipa | `SeasonDto` |
| GET | `/api/Team/{id}/titles` | público | `TeamTitleDto[]` |

```jsonc
// LeagueDto
{ "id": "…", "name": "Primeira Divisão", "level": 1, "promotionSpots": 2, "relegationSpots": 2,
  "seasonDurationDays": 120, "trophyName": "Taça da Primeira Divisão", "teamCount": 8,
  "currentSeason": { /* SeasonDto */ } }
// SeasonDto
{ "id": "…", "leagueId": "…", "name": "2026/27", "status": 1, "startDate": "…", "endDate": "…", "teamCount": 8 }
// CreateLeagueDto
{ "name": "…", "level": 1, "promotionSpots": 2, "relegationSpots": 2, "seasonDurationDays": 120, "trophyName": "…" }
// CreateSeasonDto
{ "name": "2026/27", "startDate": "2026-10-03T00:00:00Z" }
// StartSeasonDto
{ "kickoffTime": "15:00" }
// StandingsDto
{ "league": { /* LeagueDto */ }, "season": { /* SeasonDto */ } /* ou null */,
  "rows": [ { "position": 1, "teamId": "…", "teamName": "…", "icon": "…",
              "played": 10, "won": 7, "drawn": 2, "lost": 1,
              "goalsFor": 20, "goalsAgainst": 8, "goalDifference": 12, "points": 23,
              "form": ["V", "V", "E", "D", "V"],          // do mais antigo para o mais recente
              "zone": "PROMOTION" } ] }                      // "PROMOTION", "RELEGATION" ou null
// FixtureRoundDto
{ "round": 1, "matches": [ { "idMatch": "…", "date": "…", "status": 0,
    "homeTeamId": "…", "homeTeamName": "…", "awayTeamId": "…", "awayTeamName": "…",
    "homeGoals": null, "awayGoals": null } ] }
// TeamTitleDto
{ "trophyName": "Taça da Primeira Divisão", "leagueName": "Primeira Divisão", "count": 3,
  "seasons": ["2024/25", "2025/26", "2026/27"] }
```

`TeamDetailsDto` (GET `/api/Team/{id}`) ganha `creatorId`, `leagueId`, `leagueName` e `titles`
(`TeamTitleDto[]`). Os membros (`PlayerDetailsDto`) ganham `isCreator`.

### Transferências

| Método | Rota | Quem | Resposta |
|---|---|---|---|
| GET | `/api/transfers/market/{teamId}?hasTeam=&leagueId=&nationality=&position=&name=&onlyListed=` | admin da equipa | `MarketPlayerDto[]` (sem os jogadores da própria equipa) |
| POST | `/api/transfers/listings/{teamId}/{playerId}` | admin da equipa do jogador | 204 |
| DELETE | `/api/transfers/listings/{teamId}/{playerId}` | admin da equipa do jogador | 204 |
| POST | `/api/transfers/offers` | admin da equipa compradora | `TransferOfferDto` (corpo `{ "teamId", "playerId", "message" }`) |
| GET | `/api/transfers/offers/team/{teamId}` | admin da equipa | `{ "received": TransferOfferDto[], "sent": TransferOfferDto[] }` |
| GET | `/api/transfers/offers/player` | jogador autenticado | `TransferOfferDto[]` (à espera da sua resposta) |
| POST | `/api/transfers/offers/{id}/accept` | admin do clube vendedor ou o jogador | `TransferOfferDto` |
| POST | `/api/transfers/offers/{id}/reject` | clube vendedor, jogador ou clube comprador (cancela) | `TransferOfferDto` |

```jsonc
// MarketPlayerDto
{ "playerId": "…", "name": "…", "age": 24, "position": 1, "nationality": "Portugal", "imageUrl": null,
  "teamId": "…", "teamName": "…", "leagueId": "…", "leagueName": "…", "isListed": true }
// TransferOfferDto
{ "id": "…", "playerId": "…", "playerName": "…", "fromTeamId": "…", "fromTeamName": "…",
  "toTeamId": "…", "toTeamName": "…", "status": 0, "message": "…", "createdAt": "…", "decidedAt": null }
```

### Perfil do jogador

| Método | Rota | Resposta |
|---|---|---|
| GET | `/api/Player/{playerId}/profile` | `PlayerProfileDto` |
| PUT | `/api/Player/update/{playerId}` | aceita também `weight`, `preferredFoot`, `status`, `nationality`, `countryOfBirth` |

```jsonc
// PlayerProfileDto
{ "id": "…", "name": "…", "imageUrl": null, "dateOfBirth": "2000-05-01", "age": 26,
  "position": 1, "height": 178, "weight": 72, "preferredFoot": 0, "status": 0,
  "nationality": "Portugal", "countryOfBirth": "Portugal",
  "currentTeam": { "idTeam": "…", "name": "…" }, "joinedTeamAt": "…", "isListed": false,
  "totals": { "games": 12, "goals": 5, "assists": 3, "minutes": 980, "yellowCards": 2, "redCards": 0 },
  "career": [ { "season": "2026/27", "teamId": "…", "teamName": "…", "games": 12, "goals": 5,
                "assists": 3, "minutes": 980, "yellowCards": 2, "redCards": 0 } ],
  "transfers": [ { "date": "…", "fromTeamName": null, "toTeamName": "…", "kind": "ADESAO" } ] }
  // kind: "TRANSFERENCIA", "ADESAO" (entrou como jogador livre) ou "SAIDA" (saiu e ficou livre)
```

### Onze inicial

| Método | Rota | Quem | Resposta |
|---|---|---|---|
| GET | `/api/lineups/formations` | público | `FormationDto[]` |
| GET | `/api/lineups/{matchId}/{teamId}` | membros das duas equipas | `LineupDto` (o adversário só depois do prazo; antes recebe 403) |
| PUT | `/api/lineups/{matchId}/{teamId}` | admin da equipa, antes do prazo | `LineupDto` (corpo `SaveLineupDto`) |

```jsonc
// FormationDto: x e y em percentagem do campo (0–100), com a baliza da equipa em baixo (y = 100)
{ "code": "4-3-3", "slots": [ { "slot": 0, "positionCode": "GR", "role": 3, "x": 50, "y": 92 } ] }
// role usa o enum Position: FORWARD=0, MIDFIELDER=1, DEFENDER=2, GOALKEEPER=3
// LineupDto
{ "matchId": "…", "teamId": "…", "formation": "4-3-3", "deadline": "…", "isLocked": false,
  "isAutoFilled": false, "exists": true,
  "starters": [ { "slot": 0, "positionCode": "GR", "playerId": "…", "playerName": "…", "position": 3 } ],
  "bench": [ { "playerId": "…", "playerName": "…", "position": 1 } ] }
// SaveLineupDto
{ "formation": "4-3-3", "starters": [ { "slot": 0, "playerId": "…" } ], "bench": ["…"] }
```

### Resultado e relatório do jogo

`ResultMatchDto` (hub `/FinishMatch` e REST) ganha o campo opcional `events`:

```jsonc
{ "idMatch": "…", "idTeam": "…", "numGoalsTeam": 2, "idOpponent": "…", "numGoalsOpponent": 1,
  "events": { "fouls": 11,
              "goals": [ { "scorerId": "…", "assistId": null, "minute": 23 } ],
              "cards": [ { "playerId": "…", "type": 0, "minute": 40 } ],
              "substitutions": [ { "playerOutId": "…", "playerInId": "…", "minute": 60 } ] } }
```

| Método | Rota | Quem | Resposta |
|---|---|---|---|
| POST | `/api/matches/{matchId}/result` | admin de uma das equipas | `FinishMatchStateDto` (mesma lógica do hub, para o frontend web) |
| GET | `/api/matches/{matchId}/report` | público | `MatchReportDto` |

```jsonc
// FinishMatchStateDto
{ "submitted": true, "matchFinished": false, "resultsCoincide": null, "message": "…" }
// MatchReportDto
{ "matchId": "…", "date": "…", "status": 2, "isCompetitive": true, "leagueName": "…", "round": 3,
  "pitchName": "…", "home": TeamReport, "away": TeamReport }
// TeamReport
{ "teamId": "…", "teamName": "…", "goals": 2, "fouls": 11, "yellowCards": 1, "redCards": 0,
  "substitutions": 2, "lineup": LineupDto /* ou null */,
  "events": [ { "type": "GOAL", "minute": 23, "playerId": "…", "playerName": "…",
                "relatedPlayerId": "…", "relatedPlayerName": "…" } ] }
  // type: "GOAL" (relacionado = assistência), "YELLOW_CARD", "RED_CARD", "SUBSTITUTION" (jogador = sai, relacionado = entra)
```

### Calendário

`InfoMatchCalendar` (GET `/api/Calendar/{idTeam}`) ganha: `homeTeam` e `awayTeam` (`TeamStatisticsDto`),
`leagueName`, `round`, `reason` (motivo do cancelamento ou do adiamento pendente) e `postponedFrom`
(data original, se o jogo foi adiado).

| Método | Rota | Resposta |
|---|---|---|
| GET | `/api/Calendar/{idTeam}/history` | `CalendarMarkerDto[]`: cancelamentos e adiamentos passados, na data original |
| PUT | `/api/Calendar/{idTeam}/{idMatch}/cancel-reschedule` | jogo da liga: cancela e remarca (corpo `{ "reason", "newDate" }`) |

```jsonc
// CalendarMarkerDto
{ "idMatch": "…", "date": "…", "kind": "CANCELLED", "reason": "…", "opponentName": "…", "newDate": "…" }
// kind: "CANCELLED" ou "POSTPONED"
```

`PostPoneMatchDto` ganha `reason` (obrigatório na web; opcional na API para compatibilidade).

## Riscos e limites conhecidos

- **A migração da base de dados** foi gerada com `dotnet ef` e aplicada só em teste de arranque; numa base
  com dados, convém uma cópia de segurança antes de `dotnet ef database update`.
- **Resultado divergente:** se os dois administradores não chegarem a acordo, o jogo fica por terminar,
  como antes. Não há arbitragem.
- **Eventos sem confirmação cruzada:** cada equipa regista os seus próprios eventos e o adversário não os
  valida (ao contrário do resultado).
- **Minutos** assumem jogos de 90 minutos e ignoram o tempo de compensação.
