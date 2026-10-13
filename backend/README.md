# Futebol Amador — API

ASP.NET Core 8 REST API and SignalR hubs serving the web frontend and Android app. See the [main README](../README.md) for the project overview.

## Layers

| Project | Contents | Dependencies |
|---|---|---|
| `Domain` | Entities (`Team`, `Player`, `Matches`, `Rank`, etc.), enums, model constants, exceptions | — |
| `Application` | Business services, validators, DTOs, repository interfaces | Domain |
| `Infrastructure` | `AmateurFootballContext`, EF Core/SQL Server, migrations, repositories | Application, Domain |
| `Api` | REST controllers, SignalR hubs, background services, authentication, error handling | All application layers |
| `Tests` | NUnit unit tests with Moq and integration tests with `WebApplicationFactory` | All application layers |

Design decisions:

- **Two-level authorisation:** controllers use `IPlayerAuthorizationService` to check membership or administrator rights for the team in the URL. Validators enforce operation-specific rules, such as whether a membership request belongs to that team.
- **Errors:** services throw domain exceptions; `GlobalExceptionHandler` converts them into `ProblemDetails`.

| Exception or condition | HTTP response |
|---|---|
| `ValidationException`, `BusinessRuleException`, etc. | 400 |
| No session (`UnauthorizedAccessException`) | 401 |
| `ForbiddenException` | 403 |
| `NotFoundException` | 404 |
| `DbUpdateConcurrencyException` or unique constraint | 409 |
| Request body too large | 413 |
| External-service timeout handled by Polly | 503 |
| Other errors | 500 without internal messages |

Unknown routes and unsupported methods also return ProblemDetails (404/405). The 401/403 distinction matters because 401 ends the frontend session.

- **Personal data:** email, phone, address, and date of birth are returned only to the player and teammates, using `PlayerDetailsDto.OcultarDadosPessoais`.
- **Real time:** the current hub list is:

| Hub | Purpose |
|---|---|
| `/StartMatch` | Both administrators confirm kickoff |
| `/FinishMatch` | Submit scores; finish only when both agree |
| `/Notification` | Team notifications |

`CompetitionBackGroundService` opens/closes league seasons, generates fixtures, awards trophies, handles promotion/relegation, and fills missing lineups after the deadline.

- **Leagues and transfers:** `Application/Competition` contains standings, circle-method scheduling, formations, automatic lineups, and statistics; services live in `Application/Services/Competition`. See [design and API contracts](../docs/novas-funcionalidades.md).

## Run locally

Requires **.NET 8 SDK**, **SQL Server**, and a **Firebase project** with email/password Authentication, Firestore, and an Admin SDK service-account key.

The following commands assume your working directory is `backend/` and use Bash syntax.

1. Start SQL Server, for example in Docker:

   ```bash
   docker run -d --name futebol-sql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='A-Strong-Password-1' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
   ```

2. Configure user secrets outside the repository:

   ```bash
   dotnet user-secrets set --project Api "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=FutebolAmador;User Id=sa;Password=A-Strong-Password-1;TrustServerCertificate=True"
   dotnet user-secrets set --project Api "Firebase:ProjectId" "your-project"
   dotnet user-secrets set --project Api "Firebase:CredentialPath" "C:/path/to/firebase-adminsdk.json"
   dotnet user-secrets set --project Api "Authentication:TokenUri" "https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=YOUR_FIREBASE_WEB_API_KEY"
   dotnet user-secrets set --project Api "Firebase:ApiKey" "YOUR_FIREBASE_WEB_API_KEY"
   # Optional: Turnstile, signed Cloudinary uploads, and development-only unverified email
   dotnet user-secrets set --project Api "Turnstile:SecretKey" "0x..."
   dotnet user-secrets set --project Api "Cloudinary:CloudName" "your-cloud-name"
   dotnet user-secrets set --project Api "Cloudinary:ApiKey" "..."
   dotnet user-secrets set --project Api "Cloudinary:ApiSecret" "..."
   dotnet user-secrets set --project Api "Auth:RequireVerifiedEmail" "false"
   ```

   In production, use corresponding environment variables such as `ConnectionStrings__DefaultConnection` and `Firebase__ProjectId`. Set allowed origins in `Cors:Origins`; behind a proxy, add its IPs to `ForwardedHeaders:KnownProxies`. Security settings are documented in [docs/SEGURANCA.md](../docs/SEGURANCA.md).

3. Apply migrations and start:

   ```bash
   dotnet tool install --global dotnet-ef --version 9.0.10
   dotnet ef database update --project Infrastructure --startup-project Api
   dotnet run --project Api        # http://localhost:5218/swagger
   ```

   Existing division records are initialised if missing; league assignment for existing teams is described in the main README.

Build the Docker image from `backend/`:

```bash
docker build -f Api/Dockerfile -t futebol-amador-api .
```

## Tests

```bash
dotnet test
```

Integration tests start the API in `Testing` with an in-memory database, mocked Firebase, and a test authentication scheme. `Authorization: Test` identifies a fixed verified user; `Authorization: Test-NaoVerificado` simulates an unverified email. Rate limits are raised except in tests specifically checking them. No credentials are required, including in GitHub Actions.

Tests against external services are skipped when their required configuration is absent and are also configured to run in CI:

```bash
# Real SQL Server: optimistic concurrency, unique constraints, GDPR workflows
docker run -d --name fa-sql -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='A-Strong-Password-1' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
FA_TESTES_SQLSERVER="Server=localhost,1433;User Id=sa;Password=A-Strong-Password-1;TrustServerCertificate=True" dotnet test --filter Category=SqlServer

# Firestore emulator: export/delete chat messages, run from backend/
(cd ../firebase && firebase emulators:exec --only firestore --project demo-futebol-amador "dotnet test ../backend/Tests --filter Category=Firestore")
```

Create migrations without starting the API; `AmateurFootballContextFactory` avoids a Firebase dependency:

```bash
dotnet ef migrations add MigrationName --project Infrastructure --startup-project Infrastructure
```

Load testing is in `Ferramentas/Carga`; see [operations documentation](../docs/OPERACAO.md).

## Main endpoints

| Resource | Examples |
|---|---|
| Account | `POST /api/User/login`, `POST /api/User/logout`, `POST /api/User/refresh`, `PUT /api/User/password`, `POST /api/User/resend-verification`, `POST /api/User/forgot-password`, `POST /api/Player/create-profile` |
| Uploads | `POST /api/uploads/signature` for Cloudinary signatures |
| Players | `GET /api/Player/details/{id}`, `PUT /api/Player/update/{id}`, `PUT /api/Player/{id}/leave-team` |
| Teams | `POST /api/Team`, `GET/PUT/DELETE /api/Team/{id}`, `GET /api/Team/{id}/search`, `GET /api/Team/homeTeam/{id}` |
| Members | `GET /api/Team/{id}/members`, `PUT /api/Team/{id}/members/promote/{playerId}`, `DELETE /api/Team/{id}/members/{playerId}` |
| Membership requests | `GET/POST /api/Team/{id}/membership-request...`, `GET/POST /api/Player/{id}/membership-requests...` |
| Matches | `GET /api/Calendar/{idTeam}`, `PUT /api/Calendar/{idTeam}/PostponeMatch`, `DELETE /api/Calendar/{idTeam}/CancelMatch/{idMatch}` |
| Invitations | `GET/POST /api/MatchInvite/{idTeam}...`, `AcceptMatchInvite`, `RefuseMatchInvite`, `Negociate` |
| Postponements | `GET /api/Team/{id}/PostPoneMatch`, `AcceptPostponeMatch`, `RejectPostponeMatch` |
| Standings | `GET /api/Leaderboard?leagueId=`, `GET /api/leagues/{id}/standings` |
| Leagues | `GET/POST /api/leagues`, `POST /api/leagues/{id}/seasons`, `POST /api/leagues/{id}/register/{teamId}`, `GET /api/leagues/seasons/{id}/fixtures` |
| Transfers | `GET /api/transfers/market/{teamId}`, `POST/DELETE /api/transfers/listings/{teamId}/{playerId}`, `POST /api/transfers/offers`, `.../offers/{id}/accept`, `.../reject` |
| Lineups and reports | `GET/PUT /api/lineups/{matchId}/{teamId}`, `GET /api/lineups/formations`, `POST /api/matches/{id}/result`, `GET /api/matches/{id}/report` |
| Profiles | `GET /api/Player/{id}/profile`, `GET /api/Team/{id}/titles` |
| Personal data (GDPR) | `GET /api/User/me/export`, `DELETE /api/User/me`, `GET /api/User/privacy-policy` |
| Monitoring | `GET /health/live`, `GET /health/ready` |

Swagger (`/swagger` in development) provides the complete list and request/response models.