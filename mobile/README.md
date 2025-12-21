# Amateur Football Management App (LDS)

![Android](https://img.shields.io/badge/Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)
![Kotlin](https://img.shields.io/badge/Kotlin-0095D5?style=for-the-badge&logo=kotlin&logoColor=white)
![Firebase](https://img.shields.io/badge/firebase-ffca28?style=for-the-badge&logo=firebase&logoColor=black)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

## English

> [cite_start]A complete solution to connect players and teams, facilitating the organization of casual and competitive amateur football matches.

### 📱 Product Vision

Currently, there is a recurring difficulty in organizing amateur football matches, often due to a lack of players or opposing teams.

This project aims to solve this problem by acting as an intermediary to connect groups with similar interests. The application allows:
* **Casual Mode:** Find teams using filters and lists.
* **Competitive Mode:** Automatic *matchmaking* based on location, age, and history.

---

### ✨ Main Features

#### ⚽ Sports Management
* **Team Management:** Creation and editing of teams, administrator promotion, and roster management.
* **Membership System:** Players without a team can send requests to clubs (and vice-versa).
* **Match Organization:** Start, finish (with cross-result validation), postpone, and cancel matches.
* **Real-Time Matchmaking:** Use of *Lobbies* to find opponents and synchronize match start.

#### 🛠️ User Tools
* **Private Chat:** Exclusive communication between team administrators with scheduled matches.
* **Calendar Integration:** Automatic synchronization of matches with the mobile device's native calendar.
* **Smart Notifications:** Alerts for match days, rain, invitations, and schedule changes via Firebase Cloud Messaging.
* **Geolocation:** Address validation and field visualization via OpenStreetMap.

---

### 🛠 Technologies and Architecture

This project was developed with a focus on mobility and scalability, using the following tools:

#### Frontend (Mobile)
* **Android (Kotlin):** Native development.
* **CameraX:** Photo capture for profiles and teams directly in the app.
* **Libphonenumber:** International mobile number validation during registration.
* **OpenStreetMap (OSM):** Interactive maps and address validation.

#### Backend & Services
* **Public API (.NET):** Centralized backend communicating via REST and SignalR.
    * *Swagger Documentation:* [API Link](https://amfootballapi.duckdns.org/swagger/index.html).
* **SignalR:** Management of *Hubs* for real-time match start and completion.
* **Firebase Firestore:** NoSQL database with strict security rules for private data (chats) and public data (leaderboard).
* **Firebase Cloud Messaging (FCM):** Push notification system and background logic execution.
* **Cloudinary:** Cloud image storage and optimization.

---

### 📸 Screenshots

| Home Page | Team Profile | Calendar |
|:---:|:---:|:---:|
| | | |
| *Main Dashboard* | *Club Details* | *Match Schedule* |

| Chat | Matchmaking | Map (OSM) |
|:---:|:---:|:---:|
| | | |
| *Chat between Admins* | *Waiting Lobby* | *Location Selection* |

---

### 🚀 Installation and Setup

1.  **Clone the Repository:**
    ```bash
    git clone [https://github.com/seu-username/mobile-lds.git](https://github.com/seu-username/mobile-lds.git)
    ```

2.  **Configure Environment Variables:**
    * Ensure you configure the Firebase keys (google-services.json).
    * Configure Cloudinary credentials.

3.  **Compile the Project:**
    * Open the project in Android Studio.
    * Sync Gradle.
    * Run on an emulator or physical device.

---

## 📚 Documentation

The Android code documentation was generated using **Dokka**, ensuring the technical structure is always accessible and up-to-date.

---

### 👥 Authors

Work developed within the scope of the Mobile and Ubiquitous Computing (CMU) Curricular Unit:

* **Artur Gentil Silva Pinto** (Nº 8230138) 
* **Willkie Bianchi Parahyba Filho** (Nº 8230127) 

---

### 🔗 Links
- Home: https://github.com/Arturito2005/TrabalhoLDS
- Backend: https://github.com/Btx69-jpg/Backend-LDS
- Frontend Web: https://github.com/Btx69-jpg/FrontendWeb-FutebolAmador

---

### 📄 License

This project is developed for academic purposes.


## Português

> Uma solução completa para conectar jogadores e equipas, facilitando a organização de partidas de futebol amador casuais e competitivas.

### 📱 Visão do Produto

Atualmente, existe uma dificuldade recorrente na organização de jogos de futebol amador, muitas vezes devido à falta de jogadores ou equipas adversárias.

Este projeto visa resolver esse problema funcionando como um intermediário para conectar grupos com interesses semelhantes. A aplicação permite:
* **Modo Casual:** Encontrar equipas através de filtros e listas.
* **Modo Competitivo:** *Matchmaking* automático baseado em localização, idade e histórico.

---

### ✨ Funcionalidades Principais

#### ⚽ Gestão Desportiva
* **Gestão de Equipas:** Criação e edição de equipas, promoção de administradores e gestão de plantel.
* **Sistema de Adesão:** Jogadores sem equipa podem enviar pedidos a clubes (e vice-versa).
* **Organização de Partidas:** Iniciar, finalizar (com validação de resultado cruzado), adiar e cancelar jogos.
* **Matchmaking em Tempo Real:** Utilização de *Lobbies* para encontrar adversários e sincronizar o início do jogo.

#### 🛠️ Ferramentas para o Utilizador
* **Chat Privado:** Comunicação exclusiva entre administradores de equipas com partidas agendadas.
* **Integração com Calendário:** Sincronização automática dos jogos com o calendário nativo do dispositivo móvel.
* **Notificações Inteligentes:** Alertas para dias de jogo, chuva, convites e alterações de horário via Firebase Cloud Messaging.
* **Geolocalização:** Validação de moradas e visualização de campos via OpenStreetMap.

---

### 🛠 Tecnologias e Arquitetura

Este projeto foi desenvolvido com foco em mobilidade e escalabilidade, utilizando as seguintes ferramentas:

#### Frontend (Mobile)
* **Android (Kotlin):** Desenvolvimento nativo.
* [cite_start]**CameraX:** Captura de fotos para perfis e equipas diretamente na app.
* [cite_start]**Libphonenumber:** Validação internacional de números de telemóvel no registo.
* [cite_start]**OpenStreetMap (OSM):** Mapas interativos e validação de moradas.

#### Backend & Serviços
* **API Pública (.NET):** Backend centralizado que comunica via REST e SignalR.
    * *Documentação Swagger:* [Link da API](https://amfootballapi.duckdns.org/swagger/index.html).
* **SignalR:** Gestão de *Hubs* para início e finalização de partidas em tempo real.
* **Firebase Firestore:** Base de dados NoSQL com regras de segurança rigorosas para dados privados (chats) e públicos (leaderboard).
* **Firebase Cloud Messaging (FCM):** Sistema de notificações Push e execução de lógica em background.
* **Cloudinary:** Armazenamento e otimização de imagens na nuvem.

---

### 📸 Capturas de Ecrã

| Página Inicial | Perfil da Equipa | Calendário |
|:---:|:---:|:---:|
| | | |
| *Dashboard Principal* | *Detalhes do Clube* | *Agenda de Jogos* |

| Chat | Matchmaking | Mapa (OSM) |
|:---:|:---:|:---:|
| | | |
| [cite_start]*Conversa entre Admins* | *Lobby de Espera* | *Seleção de Local* |

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

A documentação do código Android foi gerada utilizando **Dokka**, garantindo que a estrutura técnica esteja sempre acessível e atualizada.

---

### 👥 Autores

Trabalho realizado no âmbito da Unidade Curricular de Computação Móvel e Ubíqua (CMU):

* **Artur Gentil Silva Pinto** (Nº 8230138) 
* **Willkie Bianchi Parahyba Filho** (Nº 8230127)

---

### 🔗 Links
- Home: https://github.com/Arturito2005/TrabalhoLDS
- Backend: https://github.com/Btx69-jpg/Backend-LDS
- Frontend Web: https://github.com/Btx69-jpg/FrontendWeb-FutebolAmador
---

### 📄 Licença

Este projeto é desenvolvido para fins académicos.
