# Segurança

Este documento descreve as proteções do Futebol Amador (API, frontend web, app Android e Firebase),
a configuração que exigem e os riscos que ficam em aberto.

## Resumo

| Área | Proteção |
|---|---|
| Autenticação | ID tokens do Firebase validados pelo JwtBearer (emissor, audiência, validade, tolerância de 1 minuto) e revogação verificada (`auth_time` contra `tokensValidAfter`, com cache de 60 s). |
| Autorização | Todos os endpoints exigem sessão **com e-mail confirmado** (`FallbackPolicy`); os públicos têm `[AllowAnonymous]`. O logout só exige sessão. |
| Abuso | Limitação de pedidos por IP (global e por endpoint sensível), `429` com `Retry-After`. Cloudflare Turnstile e campo-armadilha nos formulários públicos da web. |
| Sessão web | Tokens em cookies `HttpOnly`, `Secure`, `SameSite=Strict`; renovação no servidor; proteção CSRF (`X-Requested-With` + `Origin`). |
| Sessão Android | `Authorization: Bearer`; token e perfil cifrados com AES-GCM e chave do Android Keystore; sem cópias de segurança. |
| Contas | Confirmação do e-mail obrigatória, palavras-passe com 10–128 caracteres e 4 tipos de caracteres, reautenticação para mudar o e-mail, respostas genéricas (sem enumeração de contas). |
| Dados pessoais | Listas de jogadores só com a zona (localidade); perfil público sem data de nascimento de terceiros; contactos só para o próprio e colegas de equipa. |
| Controlo de acesso | Convites de jogo só para as equipas envolvidas; salas de chat só com colegas da mesma equipa (máx. 40 membros); resultado do jogo pela equipa do utilizador autenticado. |
| Cabeçalhos | API: CSP `default-src 'none'`, `frame-ancestors 'none'`, `nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: no-referrer`, `Permissions-Policy`, COOP/CORP, `Cache-Control: no-store`, HSTS fora de desenvolvimento, sem `Server`. Web: os mesmos em `web/deploy/nginx.conf`, com CSP compatível com a aplicação. |
| Uploads | Emblemas enviados para o Cloudinary com assinatura da API (preset assinado, pasta do utilizador, JPG/PNG/WebP, 2 MB). A API só aceita emblemas do Cloudinary por HTTPS ou imagens PNG/JPEG/WebP embebidas (até ~150 KB). |
| Firestore | Regras em `firebase/firestore.rules`: salas só para membros, mensagens só em nome próprio, sem edição/remoção, resto fechado. |
| Erros | Mensagens das exceções da framework nunca chegam ao cliente; 404/405/401/409/413/503 em ProblemDetails; logs sem e-mails, tokens nem conteúdo de notificações. |
| Quotas | Por utilizador nas operações caras (uploads, salas de chat, convites, equipas, exportações). |
| Segredos | `gitleaks` em todo o histórico no CI (`segredos.yml`); um achado conhecido e documentado abaixo. |
| Dados pessoais | Exportação e eliminação da conta, consentimento no registo, retenção automática ([RGPD.md](RGPD.md)). |

## Configuração da API

Valores sensíveis por *user-secrets* em desenvolvimento e por variáveis de ambiente em produção
(`Seccao__Chave`, por exemplo `Turnstile__SecretKey`). Nunca no `appsettings.json`.

| Chave | Obrigatória | Descrição |
|---|---|---|
| `Firebase:ProjectId`, `Firebase:CredentialPath` | sim | Projeto e conta de serviço (SDK Admin). |
| `Authentication:TokenUri` | sim | `https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=<WEB_API_KEY>`. |
| `Firebase:ApiKey` | recomendada | Chave Web do Firebase para confirmação do e-mail, recuperação da palavra-passe e renovação da sessão. Sem ela, é lida do `key` do `TokenUri`. |
| `Auth:RequireVerifiedEmail` | não (`true`) | Exige e-mail confirmado no login e em todos os endpoints protegidos. `false` só em desenvolvimento. |
| `Auth:VerificarRevogacao` | não (`true`) | Verifica sessões revogadas/contas desativadas em cada pedido. |
| `Auth:RevogacaoCacheSegundos` | não (`60`) | Tempo em cache do estado da conta. |
| `Auth:Cookies:SameSite` | não (`Strict`) | `Strict` exige web e API no mesmo site (ver abaixo). |
| `Auth:Cookies:RefreshDias` | não (`7`) | Duração do cookie do refresh token. |
| `Cors:Origins` | sim | Origens do frontend web. Também definem o que é um pedido "web" (cookies, Turnstile, CSRF). |
| `Turnstile:SecretKey` | não | Chave secreta do Cloudflare Turnstile. Vazia = verificação desligada. |
| `Cloudinary:CloudName`, `Cloudinary:ApiKey`, `Cloudinary:ApiSecret` (ou `CLOUDINARY_API_SECRET`), `Cloudinary:UploadPreset` | para uploads | Sem elas, `POST /api/uploads/signature` responde 503. |
| `ForwardedHeaders:KnownProxies` / `KnownNetworks` / `ForwardLimit` | atrás de proxy | IPs/redes do proxy reverso (ex.: `["10.0.0.0/8"]`). Sem isto o IP visto é o do proxy e a limitação de pedidos junta todos os clientes. |
| `LimitacaoPedidos:{Global,Autenticacao,Sessao,Registo,Email,PalavraPasse}:{Pedidos,JanelaSegundos}` | não | Limites por IP (por omissão 300/60 s, 10/300 s, 30/300 s, 5/3600 s, 5/900 s, 5/900 s). |
| `LimitacaoPedidos:{Uploads,SalasChat,Convites,Equipas,Exportacao}:{Pedidos,JanelaSegundos}` | não | Quotas por utilizador (20/h, 10/dia, 30/h, 3/dia, 5/h). Ver [CUSTOS.md](CUSTOS.md). |
| `Limites:*`, `BaseDados:*`, `Idempotencia:HorasRetencao`, `Cache:PublicaSegundos` | não | Tempos máximos, corpo máximo, SQL, idempotência e cache. Ver [OPERACAO.md](OPERACAO.md). |
| `Rgpd:VersaoPolitica`, `Rgpd:RetencaoAtiva`, `Rgpd:Dias*` | não | Versão da Política de Privacidade exigida no registo e prazos de retenção. Ver [RGPD.md](RGPD.md). |

Exemplo (desenvolvimento):

```bash
cd backend/Api
dotnet user-secrets set "Firebase:ApiKey" "A_WEB_API_KEY"
dotnet user-secrets set "Turnstile:SecretKey" "0x..."
dotnet user-secrets set "Cloudinary:ApiSecret" "..."
dotnet user-secrets set "Auth:RequireVerifiedEmail" "false"   # opcional, só em desenvolvimento
```

## Endpoints de conta

| Endpoint | Notas |
|---|---|
| `POST /api/User/login` | 401 genérico para credenciais erradas; 403 com `codigo: "email_nao_verificado"` só depois de a palavra-passe estar certa. Limite: 10 por 5 min por IP. |
| `POST /api/Player/create-profile` | Envia a confirmação do e-mail e responde `201` com `verificacaoEmailPendente: true`, sem sessão. Limite: 5 por hora por IP. |
| `POST /api/User/resend-verification` | `{ email, password }`; resposta sempre `202` igual. |
| `POST /api/User/forgot-password` | `{ email }`; resposta sempre `202` igual (o Firebase envia a mensagem). |
| `POST /api/User/refresh` | Só web: lê o cookie `__Secure-fa_refresh` e renova os cookies. |
| `POST /api/User/logout` | Revoga os refresh tokens no Firebase (todas as sessões) e apaga os cookies. Deixou de ser `GET`. |
| `PUT /api/User/password` | Palavra-passe atual + nova (política). Termina as outras sessões; na web os cookies passam para a sessão nova. |
| `PUT /api/Player/update/{id}` | Mudar o e-mail exige `CurrentPassword`; o e-mail novo fica por confirmar e as sessões terminam. |
| `POST /api/uploads/signature` | Assinatura para upload direto no Cloudinary. Quota: 20 por hora por utilizador. |
| `GET /api/User/me/export` | Todos os dados pessoais em JSON (RGPD). |
| `DELETE /api/User/me` | `{ password }`; elimina (anonimiza) a conta, apaga o utilizador no Firebase e os cookies. `DELETE /api/Player/{id}` responde 410. |
| `GET /api/User/privacy-policy` | Versão atual da Política de Privacidade. |
| `GET /health/live`, `GET /health/ready` | Anónimos, sem limitação de pedidos. |

## Sessão web (cookies)

- `__Host-fa_session`: ID token (1 hora), `Path=/`. O JwtBearer usa-o quando não há `Authorization`.
- `__Secure-fa_refresh`: refresh token, `Path=/api/User/refresh`.
- Ambos `HttpOnly; Secure; SameSite=Strict`. O browser só guarda em `localStorage` o id do jogador, a
  equipa e se é administrador (para o menu); não dão acesso a nada.
- **CSRF**: pedidos `POST/PUT/DELETE` com cookie e sem `Authorization` precisam de
  `X-Requested-With: FutebolAmador` **e** de um `Origin` da lista `Cors:Origins`; caso contrário `403`.
- **Mesmo site**: com `SameSite=Strict` o frontend e a API têm de estar no mesmo site. `duckdns.org` é um
  sufixo público, por isso `app.duckdns.org` e `amfootballapi.duckdns.org` são sites diferentes. Opções:
  1. servir a API no mesmo domínio do frontend (bloco `location /api/` em `web/deploy/nginx.conf` e
     `apiBaseUrl: '/api'`) — recomendado;
  2. usar um domínio próprio (`app.dominio.pt` + `api.dominio.pt`);
  3. `Auth:Cookies:SameSite=None` (menos seguro; a proteção CSRF continua ativa, mas os browsers que
     bloqueiam cookies de terceiros deixam de enviar a sessão).
- O `Cors:Origins` de produção deve conter só o(s) domínio(s) reais do frontend, com `https://`.

## Turnstile e app Android

O Turnstile protege os formulários da **web**: a API exige `X-Turnstile-Token` nos pedidos com `Origin`
(login, registo, reenvio da confirmação, recuperação). Configurar a *site key* em
`web/src/app/environments/environment.ts` (`turnstileSiteKey`) e a chave secreta em `Turnstile:SecretKey`.

A app Android não envia `Origin` e não tem Turnstile. **Risco residual:** um robô pode imitar a app (sem
`Origin`) e contornar o Turnstile; nesse caminho a proteção é a limitação de pedidos por IP e a
confirmação do e-mail. O equivalente móvel é o **Firebase App Check com Play Integrity**, que fica como
trabalho futuro (a API passaria a exigir o token do App Check nos pedidos sem `Origin`).

## Cloudinary

1. No Cloudinary, **apagar ou desativar o preset não assinado `android_upload`** (a app já não o usa).
2. Criar o preset **assinado** `equipas_assinado` (ou o nome em `Cloudinary:UploadPreset`) com:
   *Signing mode: Signed*; *Allowed formats*: `jpg, png, webp`; *Incoming transformation*
   `c_limit,w_512,h_512`; *Moderation* (opcional, por exemplo *AWS Rekognition*); sem *overwrite*;
   pasta base `equipas/`.
3. Definir `Cloudinary:CloudName`, `Cloudinary:ApiKey` e `Cloudinary:ApiSecret` na API. O segredo nunca
   vai para a app.

A API assina `allowed_formats`, `folder` (`equipas/{uid}`), `timestamp` e `upload_preset`; o Cloudinary
recusa assinaturas com mais de 1 hora. A app limita o ficheiro a 2 MB antes de enviar.

## Regras do Firestore

As regras estão em `firebase/firestore.rules` (antes não estavam no repositório). Publicação:

```bash
npm install -g firebase-tools
cd firebase
cp .firebaserc.example .firebaserc      # e pôr o id do projeto
firebase login
firebase deploy --only firestore:rules
```

Antes de publicar, confirmar no simulador de regras da consola que: um membro lê a sala e as
mensagens; um não membro não lê; ninguém cria salas pelo cliente; uma mensagem com `senderId` de outro
utilizador é recusada. Recomenda-se também ativar o App Check no Firestore.

## Frontend web — cabeçalhos

- Produção: `web/deploy/nginx.conf` + `web/deploy/seguranca-cabecalhos.inc` (CSP, HSTS, `nosniff`,
  `X-Frame-Options`, `Referrer-Policy`, `Permissions-Policy`, COOP/CORP). A CSP permite a API
  (`connect-src`), o Turnstile (`script-src`/`frame-src https://challenges.cloudflare.com`) e imagens
  do Cloudinary e `data:`. Mudando o domínio da API, atualizar `connect-src`.
- Demonstração no GitHub Pages: o Pages não permite cabeçalhos próprios, por isso a CSP vai numa
  `<meta>` (`src/index.demo.html`); `frame-ancestors` não funciona em `<meta>`.
- O CSS crítico em linha está desligado (`inlineCritical: false`) para a CSP não precisar de
  `'unsafe-inline'` em `script-src`.

## Android

- `isMinifyEnabled`/`isShrinkResources` na release, com regras em `app/proguard-rules.pro`.
- Token e perfil da sessão cifrados (`CifraLocal`, AES-256-GCM, Android Keystore). Valores antigos em
  claro são lidos e cifrados na gravação seguinte, sem migração do Room.
- `data_extraction_rules.xml` e `backup_rules.xml` excluem tudo (nuvem e transferência entre aparelhos).
- O logout chama `POST /api/User/logout`.
- Testes instrumentados: as credenciais da conta de teste passam como argumentos
  (`-Pandroid.testInstrumentationRunnerArguments.testEmail=...` e `...testPassword=...`).
- Debug e release compilados e testes unitários a passar (setembro de 2026); testar login, registo, chat, hubs e
  criação de equipa com emblema num aparelho antes de publicar (não testado em aparelho).
- Limitação: o ecrã de edição do perfil na app ainda não pede a palavra-passe atual; mudar o e-mail na
  app devolve o erro da API ("indica a palavra-passe atual"). Fazê-lo na web.

## Segredo antigo no histórico

O commit `2d910d5f` ("Autenticação: login, logout e change password", outubro de 2025) pôs um
`JwtSettings:Secret` no `backend/Api/appsettings.json`. Esse segredo é **obsoleto**: a API deixou de
emitir JWT próprios e valida só os ID tokens do Firebase (chaves públicas da Google). Confirmado que
nenhum ficheiro atual lê `JwtSettings` (`grep -r JwtSettings` sem resultados no código). O histórico não
foi reescrito; o valor deve ser considerado comprometido e **nunca reutilizado**. Se tiver sido usado
noutro sítio, deve ser trocado aí.

## Verificações recomendadas antes de publicar

- `dotnet list package --vulnerable --include-transitive` e `npm audit --omit=dev` sem problemas.
- Cabeçalhos: `curl -sI https://<api>/api/leagues` e `curl -sI https://<web>/`.
- Login web: os cookies aparecem como `HttpOnly` nas ferramentas do browser e o corpo não tem tokens.
- `firebase deploy --only firestore:rules` feito e preset não assinado do Cloudinary removido.
