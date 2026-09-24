# Futebol Amador – API

API REST e hubs SignalR do Futebol Amador, em ASP.NET Core 8. Serve o frontend web e a app Android. Visão geral do projeto no [README principal](../README.md).

## Camadas

| Projeto | O que tem | Depende de |
|---|---|---|
| `Domain` | Entidades (`Team`, `Player`, `Matches`, `Rank`, ...), enums, constantes do modelo e exceções | – |
| `Application` | Serviços (regras de negócio), validadores, DTOs e as interfaces dos repositórios | Domain |
| `Infrastructure` | `AmateurFootballContext` (EF Core, SQL Server), migrações e repositórios | Application, Domain |
| `Api` | Controllers REST, hubs SignalR, serviços em segundo plano, autenticação e tratamento de erros | todos |
| `Tests` | Testes NUnit: unitários (serviços com Moq) e de integração (`WebApplicationFactory`) | todos |

Algumas decisões:

- **Autorização em dois níveis.** Os controllers confirmam o papel do utilizador com o `IPlayerAuthorizationService` (membro ou administrador da equipa do URL). Os validadores voltam a confirmar as regras de cada operação, por exemplo que o pedido de adesão é mesmo dessa equipa.
- **Erros.** Os serviços lançam exceções de domínio e o `GlobalExceptionHandler` converte-as em `ProblemDetails`:

  | Exceção | Resposta |
  |---|---|
  | `ValidationException`, `BusinessRuleException`, ... | 400 |
  | sem sessão (`UnauthorizedAccessException`) | 401 |
  | `ForbiddenException` | 403 |
  | `NotFoundException` | 404 |
  | tudo o resto | 500, sem mostrar a mensagem interna |

  A distinção entre 401 e 403 importa para os clientes: um 401 termina a sessão no frontend.
- **Dados pessoais.** O e-mail, o telefone, a morada e a data de nascimento de um jogador só são devolvidos ao próprio e aos colegas de equipa (`PlayerDetailsDto.OcultarDadosPessoais`).
- **Tempo real.** Há quatro hubs:

  | Hub | Para quê |
  |---|---|
  | `/StartMatch` | os dois administradores confirmam o início do jogo |
  | `/FinishMatch` | cada um regista o resultado e o jogo só termina se coincidirem |
  | `/MatchMaker` | fila de jogos competitivos |
  | `/Notification` | notificações por equipa |

  O `RankMatchMakerBackGroundService` emparelha as equipas da fila a cada 5 segundos. Se não houver par, alarga os critérios aos poucos.

## Executar localmente

Requisitos:

- **.NET 8 SDK**;
- **SQL Server** (por exemplo, em Docker);
- um **projeto Firebase** com Authentication (e-mail/palavra-passe) e Firestore, e a chave de uma conta de serviço (Admin SDK).

1. Base de dados (em Docker):

   ```bash
   docker run -d --name futebol-sql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Uma-Password-Forte-1' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
   ```

2. Configuração com *user-secrets* (fica fora do repositório):

   ```bash
   cd Api
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=FutebolAmador;User Id=sa;Password=Uma-Password-Forte-1;TrustServerCertificate=True"
   dotnet user-secrets set "Firebase:ProjectId" "o-teu-projeto"
   dotnet user-secrets set "Firebase:CredentialPath" "C:/caminho/para/firebase-adminsdk.json"
   dotnet user-secrets set "Authentication:TokenUri" "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=A_WEB_API_KEY_DO_FIREBASE"
   ```

   Em produção usam-se variáveis de ambiente com os mesmos nomes (`ConnectionStrings__DefaultConnection`, `Firebase__ProjectId`, ...). As origens autorizadas pelo CORS ficam em `Cors:Origins`.

3. Migrações e arranque:

   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project Infrastructure --startup-project Api
   dotnet run --project Api        # http://localhost:5218/swagger
   ```

   No primeiro arranque são criadas as quatro divisões, se ainda não existirem.

Com Docker (a partir desta pasta):

```bash
docker build -f Api/Dockerfile -t futebol-amador-api .
```

## Testes

```bash
dotnet test
```

Os testes de integração arrancam a API no ambiente `Testing`:

- a base de dados é em memória;
- o Firebase é substituído por *mocks*;
- a autenticação usa um esquema de teste (o cabeçalho `Authorization: Test` identifica um utilizador fixo).

Por isso correm sem credenciais, também no GitHub Actions.

## Endpoints principais

| Recurso | Exemplos |
|---|---|
| Conta | `POST /api/User/login`, `GET /api/User/logout`, `PUT /api/User/password`, `POST /api/Player/create-profile` |
| Jogadores | `GET /api/Player/details/{id}`, `PUT /api/Player/update/{id}`, `PUT /api/Player/{id}/leave-team` |
| Equipas | `POST /api/Team`, `GET/PUT/DELETE /api/Team/{id}`, `GET /api/Team/{id}/search`, `GET /api/Team/homeTeam/{id}` |
| Membros | `GET /api/Team/{id}/members`, `PUT /api/Team/{id}/members/promote/{playerId}`, `DELETE /api/Team/{id}/members/{playerId}` |
| Pedidos de adesão | `GET/POST /api/Team/{id}/membership-request...`, `GET/POST /api/Player/{id}/membership-requests...` |
| Jogos | `GET /api/Calendar/{idTeam}`, `PUT /api/Calendar/{idTeam}/PostponeMatch`, `DELETE /api/Calendar/{idTeam}/CancelMatch/{idMatch}` |
| Convites | `GET/POST /api/MatchInvite/{idTeam}...`, `AcceptMatchInvite`, `RefuseMatchInvite`, `Negociate` |
| Adiamentos | `GET /api/Team/{id}/PostPoneMatch`, `AcceptPostponeMatch`, `RejectPostponeMatch` |
| Classificação | `GET /api/Leaderboard` |

A lista completa, com os modelos de pedido e resposta, está no Swagger (`/swagger`, em desenvolvimento).
