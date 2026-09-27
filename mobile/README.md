# Futebol Amador – app Android

App Android do Futebol Amador, em Kotlin com Jetpack Compose. Visão geral do projeto no [README principal](../README.md).

Foi desenvolvida por **Artur Pinto** e **Willkie Filho** para a unidade curricular de Computação Móvel e Ubíqua, sobre a mesma API do trabalho de LDS.

## Funcionalidades

- **Equipas e adesões.** Criar e editar equipas (com fotografia tirada na app, via CameraX), gerir o plantel e os administradores, e enviar ou responder a pedidos de adesão.
- **Jogos amigáveis.** Procurar adversários com filtros, enviar convites e negociar a data e o campo. Os jogos marcados podem ser adiados ou cancelados.
- **Ligas.** Classificação da liga (PD, V, E, D, GM, GS, DG, P e a forma só com ícones), com cópia no Firestore para consultar sem rede; lista de ligas com inscrição da equipa e as jornadas. Os jogos competitivos são os da liga (o *matchmaking* foi retirado).
- **Transferências.** Mercado com filtros (com ou sem equipa, liga, nacionalidade, posição), propostas recebidas e enviadas pela equipa e as propostas à espera do jogador. Os administradores colocam jogadores no mercado a partir da lista de membros.
- **Onze inicial** num campo desenhado: tática, titulares filtrados pela posição e banco; o relatório do jogo mostra as estatísticas, os acontecimentos e os dois onzes.
- **Perfil do jogador** com a ficha desportiva (pé, peso, situação, nacionalidade, clube, totais, percurso por época e transferências). O próprio edita os dados desportivos.
- **Administrador principal.** Só o criador da equipa despromove administradores; ninguém o despromove nem o expulsa.
- **Início e fim do jogo.** Os administradores das duas equipas entram num *lobby* para confirmar o início. No fim, cada um regista o resultado e os eventos da sua equipa (faltas, golos e assistências, cartões, substituições), e o jogo só termina se os resultados coincidirem.
- **Calendário em grelha mensal** com setas entre meses, feriados nacionais e bolinhas por dia (cinzento feriado, azul amigável, roxo liga, verde terminado, vermelho cancelado, amarelo adiado); tocar num dia mostra os jogos, o motivo de adiamentos e cancelamentos e as ações. Cancelar um jogo da liga pede nova data.
- **Chat** entre os administradores das equipas com jogo marcado (Firestore).
- **Notificações** (Firebase Cloud Messaging): dia de jogo, convites, adesões e alterações de horário.
- **Calendário do telemóvel.** Os jogos são sincronizados com o calendário nativo.
- **Moradas e campos** validados e mostrados no mapa com OpenStreetMap (osmdroid).
- **Números de telemóvel** validados com a libphonenumber.

## Arquitetura

- MVVM:
  - ecrãs em Compose;
  - `ViewModel`s com `StateFlow`;
  - repositórios e serviços que chamam a API com Retrofit.
- Injeção de dependências com Hilt.
- Sessão guardada localmente com Room.
- SignalR para os hubs de início e fim de jogo.
- As funcionalidades de ligas, transferências e onzes estão no pacote `competicao` (DTOs, API, ecrãs e a lógica sem Android, testada com JUnit).
- Imagens guardadas no Cloudinary.

## Configuração

1. Criar um projeto no Firebase, adicionar uma app Android com o *package* `com.example.amfootball` e copiar o `google-services.json` para `app/`. O ficheiro não está no repositório. Para apenas compilar, serve `app/google-services.example.json` copiado com esse nome.
2. Em `local.properties` (também fora do repositório), além do `sdk.dir`:

   ```properties
   API_BASE_URL=https://o-teu-servidor/
   CLOUDINARY_CLOUD_NAME=o-teu-cloud-name
   ```

   Sem `API_BASE_URL` é usado o servidor da equipa. Os emblemas são enviados para o Cloudinary com uma assinatura pedida à API (`POST api/uploads/signature`); o preset e o segredo ficam só no servidor (ver [docs/SEGURANCA.md](../docs/SEGURANCA.md)).
3. Abrir a pasta `mobile/` no Android Studio, sincronizar o Gradle e correr num emulador ou num telemóvel (Android 9 ou superior).

## Testes

```bash
./gradlew testDebugUnitTest           # testes unitários (JVM)
./gradlew connectedDebugAndroidTest   # testes instrumentados (precisa de emulador)
```

Os testes instrumentados usam Hilt e um MockWebServer em vez da API.

## Revisão de 2026

- **Crash depois de enviar, negociar ou adiar um jogo.** A app navegava para `calendar/<id>`, uma rota que não existe.
- **Posições.** O enum `Position` estava na ordem inversa da API, e um avançado ficava registado como guarda-redes. Foi acrescentado um teste unitário para esse contrato.
- **Registo.** Os erros eram ignorados e o ecrã avançava sem sessão.
- **Configuração.** A API apontava para um túnel ngrok temporário e o Cloudinary estava escrito no código. Agora os dois vêm de `local.properties` (`BuildConfig`).
- **Eventos SignalR.** Com `replay = 1`, um evento de um jogo (por exemplo, "jogo iniciado") aparecia no *lobby* do jogo seguinte.
- **Outros erros:**
  - o chat mostrava as 50 mensagens mais antigas;
  - `fetchUserId` rebentava sem sessão;
  - `allowBackup` estava ligado, com a sessão guardada na base de dados local.
- **Testes instrumentados.** Não compilavam (importavam pacotes que já não existiam) e voltaram a compilar. A versão do Hilt nos testes foi alinhada com a da app e o Jacoco passou a apontar para `src/main/java`.

## Produção

- **Rede:** OkHttp com tempos máximos (ligação 10 s, leitura/escrita 20 s, total 30 s), mensagens de erro para o
  utilizador sem detalhes técnicos (`MensagensErro`), `Idempotency-Key` em todos os `POST` e cache HTTP só para as
  respostas públicas.
- **Duplicados:** os formulários ignoram envios repetidos enquanto o anterior não termina (`FormsViewModel.aSubmeter`).
- **RGPD:** ecrã "Política de Privacidade" (acessível a partir do registo, antes de haver conta), aceitação
  obrigatória no registo, "Descarregar os meus dados" (JSON partilhado pelo seletor do Android) e eliminação da conta
  com a palavra-passe nas Definições. A localização é só a aproximada e só quando o utilizador a pede no registo.
- **Sessão:** cifra AES-GCM com chave do Android Keystore (`CifraLocal`); a parte criptográfica (`CifraAesGcm`) tem
  testes unitários.
- **Release:** R8 ativo; o `mapping.txt` fica em `app/build/outputs/mapping/release/` e não vai no APK — guardar o de
  cada versão publicada.

```bash
./gradlew --no-daemon --max-workers=1 assembleDebug assembleRelease testDebugUnitTest
```
