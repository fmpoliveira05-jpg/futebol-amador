# Futebol Amador – app Android

App Android do Futebol Amador, em Kotlin com Jetpack Compose. Visão geral do projeto no [README principal](../README.md).

Foi desenvolvida por **Artur Pinto** e **Willkie Filho** para a unidade curricular de Computação Móvel e Ubíqua, sobre a mesma API do trabalho de LDS.

## Funcionalidades

- **Equipas e adesões.** Criar e editar equipas (com fotografia tirada na app, via CameraX), gerir o plantel e os administradores, e enviar ou responder a pedidos de adesão.
- **Jogos amigáveis.** Procurar adversários com filtros, enviar convites e negociar a data e o campo. Os jogos marcados podem ser adiados ou cancelados.
- **Jogos competitivos.** Fila de *matchmaking* em tempo real (SignalR) para jogos ao domingo, com adversários de pontuação e idade média parecidas.
- **Início e fim do jogo.** Os administradores das duas equipas entram num *lobby* para confirmar o início. No fim, cada um regista o resultado e o jogo só termina se os dois coincidirem.
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
- SignalR para os hubs de início de jogo, fim de jogo e *matchmaking*.
- Imagens guardadas no Cloudinary.

## Configuração

1. Criar um projeto no Firebase, adicionar uma app Android com o *package* `com.example.amfootball` e copiar o `google-services.json` para `app/`. O ficheiro não está no repositório. Para apenas compilar, serve `app/google-services.example.json` copiado com esse nome.
2. Em `local.properties` (também fora do repositório), além do `sdk.dir`:

   ```properties
   API_BASE_URL=https://o-teu-servidor/
   CLOUDINARY_CLOUD_NAME=o-teu-cloud-name
   CLOUDINARY_UPLOAD_PRESET=android_upload
   ```

   Sem `API_BASE_URL` é usado o servidor da equipa. O *upload preset* do Cloudinary tem de ser do tipo *unsigned*.
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
