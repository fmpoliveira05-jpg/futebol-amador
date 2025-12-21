# App de Gestão de Futebol Amador (LDS)

![Android](https://img.shields.io/badge/Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)
![Kotlin](https://img.shields.io/badge/Kotlin-0095D5?style=for-the-badge&logo=kotlin&logoColor=white)
![Firebase](https://img.shields.io/badge/firebase-ffca28?style=for-the-badge&logo=firebase&logoColor=black)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)


## Português

> [cite_start]Uma solução completa para conectar jogadores e equipas, facilitando a organização de partidas de futebol amador casuais e competitivas[cite: 795, 796].

### 📱 Visão do Produto

[cite_start]Atualmente, existe uma dificuldade recorrente na organização de jogos de futebol amador, muitas vezes devido à falta de jogadores ou equipas adversárias[cite: 793, 794].

[cite_start]Este projeto visa resolver esse problema funcionando como um intermediário para conectar grupos com interesses semelhantes[cite: 796]. A aplicação permite:
* [cite_start]**Modo Casual:** Encontrar equipas através de filtros e listas[cite: 799].
* [cite_start]**Modo Competitivo:** *Matchmaking* automático baseado em localização, idade e histórico[cite: 800].

---

### ✨ Funcionalidades Principais

#### ⚽ Gestão Desportiva
* [cite_start]**Gestão de Equipas:** Criação e edição de equipas, promoção de administradores e gestão de plantel [cite: 807-813].
* [cite_start]**Sistema de Adesão:** Jogadores sem equipa podem enviar pedidos a clubes (e vice-versa)[cite: 814, 820].
* [cite_start]**Organização de Partidas:** Iniciar, finalizar (com validação de resultado cruzado), adiar e cancelar jogos [cite: 832-836, 1203].
* [cite_start]**Matchmaking em Tempo Real:** Utilização de *Lobbies* para encontrar adversários e sincronizar o início do jogo [cite: 1192-1195].

#### 🛠️ Ferramentas para o Utilizador
* [cite_start]**Chat Privado:** Comunicação exclusiva entre administradores de equipas com partidas agendadas[cite: 837, 838].
* [cite_start]**Integração com Calendário:** Sincronização automática dos jogos com o calendário nativo do dispositivo móvel[cite: 1232, 1233].
* [cite_start]**Notificações Inteligentes:** Alertas para dias de jogo, chuva, convites e alterações de horário via Firebase Cloud Messaging[cite: 850, 1249].
* [cite_start]**Geolocalização:** Validação de moradas e visualização de campos via OpenStreetMap[cite: 1256, 1258].

---

### 🛠 Tecnologias e Arquitetura

Este projeto foi desenvolvido com foco em mobilidade e escalabilidade, utilizando as seguintes ferramentas:

#### Frontend (Mobile)
* **Android (Kotlin):** Desenvolvimento nativo.
* [cite_start]**CameraX:** Captura de fotos para perfis e equipas diretamente na app[cite: 1225, 1226].
* [cite_start]**Libphonenumber:** Validação internacional de números de telemóvel no registo[cite: 1215, 1216].
* [cite_start]**OpenStreetMap (OSM):** Mapas interativos e validação de moradas[cite: 1256].

#### Backend & Serviços
* [cite_start]**API Pública (.NET):** Backend centralizado que comunica via REST e SignalR[cite: 1188, 1189].
    * [cite_start]*Documentação Swagger:* [Link da API](https://amfootballapi.duckdns.org/swagger/index.html)[cite: 1190].
* [cite_start]**SignalR:** Gestão de *Hubs* para início e finalização de partidas em tempo real[cite: 1191, 1192].
* [cite_start]**Firebase Firestore:** Base de dados NoSQL com regras de segurança rigorosas para dados privados (chats) e públicos (leaderboard)[cite: 1265, 1278].
* [cite_start]**Firebase Cloud Messaging (FCM):** Sistema de notificações Push e execução de lógica em background[cite: 1248, 1253].
* [cite_start]**Cloudinary:** Armazenamento e otimização de imagens na nuvem[cite: 1207, 1208].

---

### 📸 Capturas de Ecrã

| Página Inicial | Perfil da Equipa | Calendário |
|:---:|:---:|:---:|
| | | |
| [cite_start]*Dashboard Principal* [cite: 886] | [cite_start]*Detalhes do Clube* [cite: 1066] | [cite_start]*Agenda de Jogos* [cite: 1034] |

| Chat | Matchmaking | Mapa (OSM) |
|:---:|:---:|:---:|
| | | |
| [cite_start]*Conversa entre Admins* [cite: 1152] | [cite_start]*Lobby de Espera* [cite: 1196] | [cite_start]*Seleção de Local* [cite: 1263] |

---

### 🚀 Instalação e Configuração

1.  **Clonar o Repositório:**
    ```bash
    git clone [https://github.com/seu-username/mobile-lds.git](https://github.com/seu-username/mobile-lds.git)
    ```

2.  **Configurar Variáveis de Ambiente:**
    * Certifique-se de configurar as chaves do Firebase (google-services.json).
    * Configurar credenciais do Cloudinary.

3.  **Compilar o Projeto:**
    * Abrir o projeto no Android Studio.
    * Sincronizar o Gradle.
    * Executar num emulador ou dispositivo físico.

---

## 📚 Documentação

[cite_start]A documentação do código Android foi gerada utilizando **Dokka**, garantindo que a estrutura técnica esteja sempre acessível e atualizada[cite: 1287].

---

### 👥 Autores

[cite_start]Trabalho realizado no âmbito da Unidade Curricular de Computação Móvel e Ubíqua (CMU)[cite: 653]:

* [cite_start]**Artur Gentil Silva Pinto** (Nº 8230138) [cite: 655]
* [cite_start]**Willkie Bianchi Parahyba Filho** (Nº 8230127) [cite: 656]

---

### 🔗 Links
- Home: https://github.com/Arturito2005/TrabalhoLDS
- Backend: https://github.com/Btx69-jpg/Backend-LDS
- Frontend Web: https://github.com/Btx69-jpg/FrontendWeb-FutebolAmador
---

### 📄 Licença

Este projeto é desenvolvido para fins académicos.
