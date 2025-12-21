# LDS_25_26
## English

### 📖 Description
This project is a robust backend system designed following **Clean Architecture** principles. It focuses on scalability and maintainability by strictly separating concerns between the domain logic, application orchestration, and external infrastructure.

The system includes features such as match validation, ranking systems, administrator management, and real-time communication.

---

### 🏗 Architecture
The solution is divided into the following layers to ensure loose coupling:

* **Domain**
    * Contains the core entities and enterprise business rules.
    * **No dependencies** on external libraries (EF Core, API, etc.) or frameworks.
    * Pure C# logic.

* **Application**
    * Orchestrates business logic using the Domain layer and Infrastructure interfaces.
    * Handles Use Cases such as: Match creation validation, ranking rules, and administrator management.

* **Infrastructure**
    * Implementation of external technologies and interfaces defined in the Application layer.
    * **Database:** Entity Framework Core.
    * **Caching:** Redis.
    * **Background Jobs:** Hangfire.
    * **Real-time:** SignalR (Chat).
    * **Notifications:** Email and Push notifications services.

* **Api**
    * The entry point of the application.
    * Exposes **REST endpoints**.
    * Integrates Application and Infrastructure services (Dependency Injection).
    * Connects frontends (Web and Mobile).

* **Tests**
    * Contains Unit and Integration tests to ensure application quality and stability.

---

### 🚀 Technologies
* **.NET Core / .NET 8+** (Assumed based on description)
* **Entity Framework Core**
* **Redis**
* **Hangfire**
* **SignalR**

---

### 📦 Installation

```bash
# Clone the repository
git clone [https://github.com/your-username/LDS_25_26.git](https://github.com/your-username/LDS_25_26.git)

# Navigate to the project directory
cd LDS_25_26

# Restore dependencies
dotnet restore
```

## Português
### 📖 Descrição
Este projeto consiste num sistema backend robusto, desenvolvido seguindo os princípios de **Clean Architecture**. O foco principal é a escalabilidade e a manutenibilidade, separando estritamente as responsabilidades entre a lógica de domínio, a orquestração da aplicação e a infraestrutura externa.

O sistema inclui funcionalidades como validação de partidas, sistemas de ranking, gestão de administradores e comunicação em tempo real.

---

### 🏗 Arquitetura
A solução está dividida nas seguintes camadas para garantir um baixo acoplamento:

* **Domain (Domínio)**
    * Contém as entidades centrais e as regras de negócio empresariais.
    * **Sem dependências** de bibliotecas externas (EF Core, API, etc.) ou frameworks.
    * Lógica C# pura.

* **Application (Aplicação)**
    * Orquestra a lógica de negócio utilizando o Domínio e interfaces de infraestrutura.
    * Gere Casos de Uso como: Validação na criação de partidas, regras de ranking e gestão de administradores.

* **Infrastructure (Infraestrutura)**
    * Implementação de tecnologias externas e das interfaces definidas na camada de Aplicação.
    * **Base de Dados:** Entity Framework Core.
    * **Cache:** Redis.
    * **Jobs em Background:** Hangfire.
    * **Tempo Real:** SignalR (Chat).
    * **Notificações:** Serviços de envio de Email e Push.

* **Api**
    * O ponto de entrada da aplicação.
    * Expõe **endpoints REST**.
    * Integra os serviços de Aplicação e Infraestrutura (Injeção de Dependência).
    * Conecta os frontends (Web e Mobile).

* **Tests (Testes)**
    * Contém testes unitários e de integração para garantir a qualidade e estabilidade da aplicação.

---

### 🚀 Tecnologias
* **.NET Core / .NET 8+**
* **Entity Framework Core**
* **Redis**
* **Hangfire**
* **SignalR**

---

### 📦 Instalação

```bash
# Clonar o repositório
git clone [https://github.com/teu-usuario/LDS_25_26.git](https://github.com/teu-usuario/LDS_25_26.git)

# Navegar para a diretoria do projeto
cd LDS_25_26

# Restaurar dependências
dotnet restore
```
