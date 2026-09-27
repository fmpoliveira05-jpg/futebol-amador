# Futebol Amador

Plataforma para organizar futebol amador: equipas, ligas com subidas e descidas, transferências, convites de jogo, calendário, onze inicial e estatísticas dos jogadores. Tem uma API em ASP.NET Core, um frontend web em Angular e uma app Android em Kotlin, que partilham a mesma base de dados e o mesmo Firebase.

Trabalho de grupo de **Laboratório de Desenvolvimento de Software** (3.º ano da Licenciatura em Engenharia Informática, ESTG – Politécnico do Porto, 2025/26), revisto em 2026. A app Android foi desenvolvida em paralelo para a unidade curricular de Computação Móvel e Ubíqua.

[![Backend](https://github.com/fmpoliveira05-jpg/futebol-amador/actions/workflows/backend.yml/badge.svg)](https://github.com/fmpoliveira05-jpg/futebol-amador/actions/workflows/backend.yml)
[![Web](https://github.com/fmpoliveira05-jpg/futebol-amador/actions/workflows/web.yml/badge.svg)](https://github.com/fmpoliveira05-jpg/futebol-amador/actions/workflows/web.yml)
[![Android](https://github.com/fmpoliveira05-jpg/futebol-amador/actions/workflows/android.yml/badge.svg)](https://github.com/fmpoliveira05-jpg/futebol-amador/actions/workflows/android.yml)

**Experimentar sem instalar nada:** [demonstração web](https://fmpoliveira05-jpg.github.io/futebol-amador/) (dados fictícios em memória; qualquer e-mail e palavra-passe servem).

![Página inicial de um administrador de equipa: próximos jogos e últimos resultados](docs/capturas/web-painel.png)

## Equipa

| | | |
|---|---|---|
| Artur Pinto | backend e app Android | |
| Willkie Filho | backend e app Android | |
| Ian Costa | backend e frontend web | |
| Francisco Oliveira | backend e frontend web | [@fmpoliveira05-jpg](https://github.com/fmpoliveira05-jpg) |

Os três repositórios originais (backend, web e Android) foram juntados neste, com o histórico completo, por isso o contributo de cada um continua visível no separador *Commits*.

**O meu papel:**

- No **backend**:
  - os pedidos de adesão a equipas, com os filtros de pesquisa;
  - a autenticação (login, logout e alteração da palavra-passe);
  - a classificação geral;
  - boa parte dos testes unitários dos serviços de equipas, jogos e convites.
- No **frontend web**:
  - login e registo;
  - perfil de jogador e pedidos de adesão;
  - lista de jogadores sem equipa;
  - calendário e cancelamento de jogos;
  - a documentação Compodoc.
- Na **revisão de 2026**:
  - juntei os três repositórios e limpei segredos do histórico;
  - corrigi as falhas de segurança e os erros descritos mais abaixo;
  - reescrevi o frontend web com a interface nova, o modo demonstração e os testes;
  - pus os testes de integração do backend a correr no CI.

A app Android é sobretudo do Artur e do Willkie; na revisão corrigi-lhe erros e, depois, acrescentei os ecrãs das ligas, transferências e onzes.

## O que faz

- **Equipas.** Qualquer jogador cria uma equipa com o campo onde joga e fica administrador (até 4 por equipa).
  - Os administradores aceitam pedidos de adesão e convidam jogadores sem equipa.
  - Também promovem e removem membros.
- **Jogos amigáveis.**
  - Um administrador procura adversários (por divisão, cidade, pontos, idade média ou número de jogadores) e envia um convite com data e campo.
  - O adversário aceita, recusa ou faz uma contraproposta.
  - Depois de marcado, o jogo pode ser adiado (o adversário tem de aceitar) ou cancelado com um motivo.
- **Ligas.** As equipas inscrevem-se numa liga e, no início da época, o calendário é sorteado a duas voltas, alternando casa e fora.
  - Todos os jogos da liga são competitivos; os amigáveis não dão pontos.
  - Classificação com PD, V, E, D, GM, GS, DG e P (3/1/0) e a forma dos últimos cinco jogos.
  - A época termina ao fim de X dias: o campeão recebe o troféu (o perfil da equipa mostra "x3" por troféu) e há subidas e descidas de escalão.
  - Um jogo da liga pode ser remarcado por acordo; se for cancelado, tem de ter nova data.
- **Transferências, sem dinheiro.** O clube coloca um jogador no mercado e outra equipa fica com ele, ou outra equipa faz uma proposta: o jogador muda quando o clube e ele aceitam. O mercado filtra por equipa, liga, nacionalidade e posição e inclui os jogadores livres.
- **Onze inicial.** Até duas horas antes do jogo, o administrador escolhe a tática num campo, os titulares (filtrados pela posição) e o banco. Se não o fizer, o onze é preenchido automaticamente.
- **Resultados.** No fim do jogo, os administradores das duas equipas registam o resultado em tempo real (SignalR) e o jogo só termina quando coincidem. Cada um regista também as faltas, os golos com assistência, os cartões e as substituições da sua equipa, que dão o relatório do jogo e os minutos jogados.
- **Perfil do jogador** ao estilo do ZeroZero: pé, peso, situação, nacionalidade, clube atual e desde quando, percurso por época (jogos, golos, assistências, minutos, cartões) e transferências.
- **Administrador principal.** Quem cria a equipa é o único que despromove administradores, e ninguém o despromove a ele.
- **Calendário** em grelha mensal, com feriados e bolinhas por tipo e estado do jogo; cada dia mostra os detalhes, incluindo o motivo de adiamentos e cancelamentos.
- **Tempo real.** Chat de cada jogo (Firestore), notificações push (Firebase Cloud Messaging) e hubs SignalR para iniciar e terminar jogos.

## Arquitetura

```mermaid
flowchart LR
    web["Web<br/>Angular 20"] -->|REST + JWT| api
    android["Android<br/>Kotlin · Compose"] -->|REST + JWT| api
    android -->|SignalR| api
    subgraph api["API · ASP.NET Core 8"]
      direction TB
      c[Controllers e hubs] --> s[Application<br/>serviços e validadores]
      s --> d[Domain<br/>entidades]
      s --> i[Infrastructure<br/>EF Core, repositórios]
    end
    i --> sql[(SQL Server)]
    api -->|Admin SDK| fb[Firebase<br/>Auth · Firestore · FCM]
    android -->|chat| fb
```

- **Backend** em Clean Architecture: `Domain` (entidades e regras), `Application` (serviços, validadores e interfaces dos repositórios), `Infrastructure` (EF Core e SQL Server) e `Api` (controllers REST, hubs SignalR e serviços em segundo plano).
- **Autenticação** com Firebase Authentication. A API valida os ID tokens do Firebase como JWT e o uid identifica o jogador na base de dados.
- **Web** em Angular 20 sem zone.js: componentes *standalone*, estado em *signals*, rotas com *lazy loading* e interceptor e *guard* funcionais.
- **Android** em Kotlin com Jetpack Compose, MVVM, Hilt, Retrofit, Room (sessão local) e SignalR.

## Estrutura

```
backend/   API ASP.NET Core 8 (Clean Architecture) + testes NUnit (unitários e de integração)
web/       frontend Angular 20 + testes Jasmine/Karma e Playwright
mobile/    app Android (Kotlin, Jetpack Compose)
docs/      segurança, RGPD, operação, custos, capturas e o desenho das ligas (novas-funcionalidades.md)
operacao/  cópias de segurança do SQL Server e exportação do Firestore (com testes de restauro)
firebase/  regras do Firestore e configuração dos emuladores
```

Cada pasta tem o seu README com os detalhes e as instruções.

## Como executar

**Só o frontend, com dados de exemplo** (Node.js 20 ou superior):

```bash
cd web
npm ci
npm run demo        # abre em http://localhost:4200
```

**Sistema completo:** precisa de SQL Server e de um projeto Firebase. Os passos estão no [README do backend](backend/README.md), no [do web](web/README.md) e no [do Android](mobile/README.md).

**Testes:**

```bash
cd backend && dotnet test               # unitários e de integração (base de dados em memória, sem Firebase)
FA_TESTES_SQLSERVER="Server=localhost,1433;User Id=sa;Password=...;TrustServerCertificate=True" \
  dotnet test --filter Category=SqlServer   # concorrência e restrições únicas num SQL Server real
cd firebase && firebase emulators:exec --only firestore --project demo-futebol-amador \
  "dotnet test ../backend/Tests --filter Category=Firestore"   # chat (RGPD) no emulador
cd web && npm run test:ci && npx playwright test   # inclui cookies e armazenamento do browser
cd mobile && ./gradlew testDebugUnitTest
./operacao/sqlserver/testar-restauro.sh            # cópia de segurança e restauro
```

## O que mudou na revisão de 2026

### Repositório

- Os três repositórios foram juntados num só, com o histórico completo de todos os autores.
- Foram retirados do histórico:
  - as passwords da base de dados;
  - as chaves do Firebase;
  - tokens de teste;
  - as credenciais usadas nos testes do Cypress;
  - pastas geradas (`bin/`, `obj/`, `node_modules/`, relatórios de cobertura).
- CI com um workflow por pasta, que só corre quando essa pasta muda. Os testes de integração do backend e os testes web não precisam de segredos.

### Segurança (backend)

- Qualquer utilizador autenticado podia **editar o perfil de outro**: o id do jogador vinha no corpo do pedido.
- Nos **pedidos de adesão** havia três falhas:
  - as verificações de administrador estavam comentadas;
  - um administrador de uma equipa podia aceitar pedidos de outra;
  - um jogador podia aceitar o seu próprio pedido e entrar na equipa sem aprovação.
- Qualquer pessoa, **mesmo sem sessão**, podia criar um super administrador.
- O e-mail, o telefone e a morada dos jogadores eram **públicos** (perfil, plantel e lista de membros). Agora só os vêem o próprio jogador e os colegas de equipa.
- A alteração da palavra-passe era um `GET` com as palavras-passe no URL.
- O `GlobalExceptionHandler` nunca chegava a ser usado. Todos os erros de negócio saíam como 500, com a mensagem interna. Agora há 400, 401, 403 e 404 e os 500 não mostram detalhes.

### Segurança (revisão de setembro de 2026)

Limitação de pedidos, confirmação do e-mail obrigatória, sessão web em cookies `HttpOnly` com proteção CSRF, logout com revogação, cabeçalhos de segurança (API e web), Turnstile, uploads do Cloudinary assinados pela API, regras do Firestore no repositório, menos dados pessoais nas listas e na app Android sessão cifrada e build de release reduzida. Configuração, riscos em aberto e passos de publicação em [docs/SEGURANCA.md](docs/SEGURANCA.md).

### Pronto para produção (setembro de 2026)

- **Erros:** páginas 404 e de erro na web; API com ProblemDetails em português para 404/405/409/413/503.
- **Tempos máximos e resiliência:** 15 s na web (uma nova tentativa só em GET), OkHttp com limites na app,
  Polly nos serviços externos, limites do Kestrel e do SQL.
- **Duplicados e concorrência:** `Idempotency-Key`, botões bloqueados durante os pedidos, restrições únicas e
  `rowversion` (testados com SQL Server real: aceitar o mesmo convite 4 vezes em simultâneo cria um só jogo).
- **Desempenho:** paginação, índices, consultas sem N+1, cache das consultas públicas; teste de carga com 100
  utilizadores virtuais: p95 137 ms, 0 erros.
- **Monitorização e cópias:** `/health/live` e `/health/ready`, workflow `uptime.yml`, scripts de backup/restauro do
  SQL Server testados e exportação do Firestore testada com o emulador.
- **RGPD:** Política de Privacidade, consentimento no registo, exportação e eliminação da conta (web e app),
  retenção automática, cookies só essenciais, testes do armazenamento do browser.
- **Quotas e custos:** limites por utilizador nas operações caras e guia de orçamentos.

Detalhes em [docs/OPERACAO.md](docs/OPERACAO.md), [docs/RGPD.md](docs/RGPD.md) e [docs/CUSTOS.md](docs/CUSTOS.md).

### Erros corrigidos

- **Backend:**
  - O *matchmaking* competitivo nunca funcionava:
    - as horas até ao jogo eram calculadas ao contrário;
    - os jogos emparelhados não eram guardados;
    - as equipas ficavam na fila;
    - o cálculo da divisão rebentava nas divisões mais baixas.
  - Recusar um adiamento cancelava o jogo.
  - Um jogo terminado podia ser terminado outra vez, com pontos em dobro, e os amigáveis também davam pontos.
  - Outros erros:
    - a idade dos jogadores vinha em dias;
    - a lista de membros rebentava sempre;
    - o hub de notificações não conseguia arrancar;
    - o chat dos convites perdia-se ao criar o jogo.
- **Web:**
  - A pesquisa de equipas chamava uma rota que não existe e os pedidos de adesão enviavam dados no formato errado.
  - O build de desenvolvimento usava sempre o ambiente de produção.
  - A sessão não expirava.
  - O Avançado (posição 0) era tratado como "sem posição".
  - Vários ecrãs não atualizavam porque a app corre sem zone.js e o estado não estava em *signals*.
- **Android:**
  - A app fechava depois de enviar um convite, porque navegava para uma rota inexistente.
  - As posições estavam na ordem inversa da API: um avançado ficava registado como guarda-redes.
  - Os erros de registo eram ignorados.
  - O endereço da API era um túnel ngrok temporário.

### Frontend web

- Interface nova:
  - estilos partilhados;
  - menu que muda com o papel do utilizador e funciona em telemóvel;
  - página inicial com os próximos jogos e a forma recente;
  - páginas novas de classificação, pedidos de adiamento e alteração da palavra-passe.
- **Modo demonstração:** um interceptor responde à API com dados em memória. Serve para experimentar a aplicação, para os testes de ponta a ponta e para a versão publicada no GitHub Pages.
- Testes: 50 unitários (Jasmine/Karma) e 9 de ponta a ponta com Playwright (em desktop e telemóvel), em vez dos testes Cypress que dependiam da API de produção.

### Ligas, transferências e onzes

Acrescentados depois da revisão, nas três partes (API, web e Android). As decisões e o contrato da API estão em [docs/novas-funcionalidades.md](docs/novas-funcionalidades.md).

- As ligas substituem as divisões e o *matchmaking* (a fila de jogos competitivos e o respetivo hub deixaram de existir).
- Migração `LigasTransferenciasEOnzes`: só acrescenta tabelas e colunas. No arranque, as equipas existentes são distribuídas por ligas a partir das divisões antigas.
- Testes: backend com 407 testes a passar e 3 ignorados; web com 63 unitários e 12 cenários de ponta a ponta; Android com testes JUnit da lógica do calendário, do onze e dos eventos.

## Limitações conhecidas

- O sistema completo precisa de um projeto Firebase próprio (Authentication, Firestore e Cloud Messaging) e de um SQL Server. Sem eles só funciona o modo demonstração do frontend.
- A sessão da app Android é lida da base de dados local no *thread* principal (`allowMainThreadQueries`), porque é usada por interceptores síncronos.
- Na app Android, os testes unitários cobrem o contrato das posições e a lógica das ligas (feriados, grelha do mês, onze, eventos); os ecrãs não têm testes. Os testes instrumentados precisam de um emulador e não correm no CI.
- Os ecrãs novos da app Android (ligas, mercado, onze, relatório, ficha do jogador) só têm texto em português, mesmo com o telemóvel em inglês.
- Os eventos de cada jogo são registados por cada equipa e o adversário não os confirma (ao contrário do resultado).
- A app Android não tem Turnstile nem App Check: os pedidos sem `Origin` só estão protegidos pela limitação de pedidos (ver [docs/SEGURANCA.md](docs/SEGURANCA.md)).
