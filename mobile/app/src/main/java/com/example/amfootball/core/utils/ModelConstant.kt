package com.example.amfootball.core.utils

/**
 * Constantes globais para o controlo de paginação e dimensionamento de listas na aplicação.
 *
 * Este objeto centraliza os valores usados para limitar a quantidade de dados carregados
 * da API ou exibidos na UI, garantindo consistência no comportamento de "Infinite Scroll"
 * ou botões de "Ver Mais".
 */
object ListsSizesConst {
    /**
     * Define o número inicial de itens a serem carregados ou exibidos quando a lista
     * é aberta pela primeira vez.
     *
     * Valor atual: 10
     */
    const val INICIAL_SIZE = 10

    /**
     * Define a quantidade de novos itens a serem adicionados à lista a cada
     * pedido de paginação (ação de carregar mais dados).
     *
     * Valor atual: 10
     */
    const val INCREMENT_SIZE = 10
}

/**
 * Objeto que contém constantes de validação de uso geral na aplicação.
 *
 * Utilizado para validações transversais como endereços, nomes de cidades e
 * limites genéricos de pontuação.
 */
object GeneralConst {
    /** Número mínimo de caracteres para um endereço físico. */
    const val MIN_ADDRESS_LENGTH = 5

    /** Número máximo de caracteres para um endereço físico. */
    const val MAX_ADDRESS_LENGTH = 250

    /** Número mínimo de caracteres para o nome de uma cidade. */
    const val MIN_CITY_LENGTH = 1

    /** Número máximo de caracteres para o nome de uma cidade. */
    const val MAX_CITY_LENGTH = 50

    /** Número mínimo de golos possíveis numa partida (zero). */
    const val MIN_GOALS = 0

    /** Limite máximo de validação para golos numa partida (para evitar erros de input). */
    const val MAX_GOALS = 100
}

/**
 * Constantes relacionadas com a validação e regras de negócio do Utilizador (User).
 *
 * Define limites para dados pessoais, autenticação e restrições de idade.
 */
object UserConst {
    /** Comprimento mínimo para o nome do utilizador. */
    const val MIN_NAME_LENGTH = 3

    /** Comprimento máximo para o nome do utilizador. */
    const val MAX_NAME_LENGTH = 100

    /** Comprimento mínimo para um endereço de email. */
    const val MIN_EMAIL_LENGTH = 4

    /** Comprimento máximo para um endereço de email (padrão RFC). */
    const val MAX_EMAIL_LENGTH = 256

    /** Comprimento mínimo exigido para a palavra-passe (segurança). */
    const val MIN_PASSWORD_LENGTH = 8

    /** Comprimento máximo permitido para a palavra-passe. */
    const val MAX_PASSWORD_LENGTH = 18

    /** Tamanho fixo esperado para um número de telemóvel (excluindo indicativo). */
    const val SIZE_PHONE_NUMBER = 9

    /** Idade mínima permitida para registo na aplicação. */
    const val MIN_AGE = 18

    /** Idade máxima permitida (limite lógico da aplicação). */
    const val MAX_AGE = 70
}

/**
 * Constantes para gestão e validação de Equipas (Teams).
 *
 * Inclui regras para nomes, descrições, composição de membros e estatísticas.
 */
object TeamConst {
    /** Comprimento mínimo para o nome da equipa. */
    const val MIN_NAME_LENGTH = 3

    /** Comprimento máximo para o nome da equipa. */
    const val MAX_NAME_LENGTH = 50

    /** Comprimento máximo para a descrição/biografia da equipa. */
    const val MAX_DESCRIPTION_LENGTH = 250

    /** Número mínimo de administradores que uma equipa deve ter. */
    const val MIN_ADMINS = 1

    /** Número máximo de administradores permitidos por equipa. */
    const val MAX_ADMINS = 4

    /** Número mínimo de membros numa equipa. */
    const val MIN_MEMBERS = 1

    const val MIN_MEMBERS_TO_MATCH = 11

    /** Lotação máxima de membros numa equipa. */
    const val MAX_MEMBERS = 32

    /** Pontuação mínima possível no ranking. */
    const val MIN_NUMBER_POINTS = 0

    /** Pontuação máxima possível (limite do tipo Inteiro). */
    const val MAX_NUMBER_POINTS = Int.MAX_VALUE

    /** Idade média mínima da equipa (baseada em [UserConst.MIN_AGE]). */
    const val MIN_AVERAGE_AGE = UserConst.MIN_AGE

    /** Idade média máxima da equipa (baseada em [UserConst.MAX_AGE]). */
    const val MAX_AVERAGE_AGE = UserConst.MAX_AGE
}

/**
 * Constantes específicas para atributos de Jogadores (Players).
 *
 * Define limites físicos e de texto para perfis de jogadores.
 */
object PlayerConst {
    /** Comprimento máximo para o nome da posição (ex: "Guarda-Redes"). */
    const val MAX_POSITION_LENGTH = 12

    /** Altura mínima válida em centímetros. */
    const val MIN_HEIGHT = 100

    /** Altura máxima válida em centímetros. */
    const val MAX_HEIGHT = 250
}

/**
 * Constantes para validação de Campos de Futebol (Pitches).
 */
object PitchConst {
    /** Comprimento mínimo para o nome do campo. */
    const val MIN_NAME_LENGTH = 3

    /** Comprimento máximo para o nome do campo. */
    const val MAX_NAME_LENGTH = 50
}

/**
 * Constantes para validação de mensagens de chat ou sistema.
 */
object MessageConst {
    /** Uma mensagem não pode estar vazia (mínimo 1 caracter). */
    const val MIN_MESSAGE_LENGTH = 1

    /** Limite de caracteres por mensagem enviada. */
    const val MAX_MESSAGE_LENGTH = 250
}

/**
 * Constantes para a Tabela de Classificação (Leaderboard) de equipas.
 */
object TeamLeaderBoardConst {
    /** A primeira posição na tabela (Ranking #1). */
    const val FIRST_POSITION = 1

    /** A última posição visível ou considerada na tabela. */
    const val LAST_POSITION = 100
}

/**
 * Constantes específicas para o fluxo de finalizar uma partida.
 *
 * Embora semelhante a [GeneralConst], é usado especificamente no contexto
 * de registo de resultados finais.
 */
object FinishMatchConst {
    /** Mínimo de golos a registar. */
    const val MIN_GOALS = 0

    /** Máximo de golos a registar num resultado final. */
    const val MAX_GOALS = 100
}

/**
 * Constantes especificas para as partidas de futebol
 * */
object MatchConsts {
    /** Tamanho Mínimo da mensagem do Motivo de Cancelamento da Partida*/
    const val MIN_CANCEL_REASON_LENGTH = 5

    /** Tamanho Máximo da mensagem do Motivo de Cancelamento da Partida*/
    const val MAX_CANCEL_REASON_LENGTH = 50

    /** Tempo máximo em dias que o jogador tem antes da partida para poder cancela-la*/
    const val MAX_HOURS_TO_CANCEL = 48

    /** Tempo máximo em dias que o jogador tem antes da partida para poder adia-la*/
    const val MAX_HOURS_TO_POST_PONE = 48
}

/**
 * Objeto utilitário que centraliza as constantes de rota base (Endpoints) da API REST.
 *
 * Estas constantes são utilizadas nas interfaces do Retrofit (`@GET`, `@POST`, etc.) para definir
 * os caminhos dos recursos, evitando a duplicação de strings "hardcoded" e facilitando a
 * manutenção caso a estrutura de URLs do backend sofra alterações.
 *
 * Exemplo de uso: `@GET("${BaseEndpoints.teamApi}/details/{id}")`
 */
object BaseEndpoints {

    /**
     * O prefixo raiz para todas as chamadas à API (ex: `https://dominio.com/api`).
     */
    const val API = "api"

    /**
     * Rota base para operações relacionadas com a conta de Utilizador genérica.
     * Geralmente utilizada para Autenticação (Login, Registo de conta base).
     * Valor: `api/User`
     */
    const val AUTH_API = "$API/User"

    /**
     * Rota base para operações relacionadas com o Calendário de Jogos.
     * Inclui listagem de jogos, agendamento e resultados.
     * Valor: `api/Calendar`
     */
    const val CALENDAR_API = "$API/Calendar"

    /**
     * Rota base para operações de Chat e Mensagens.
     * Inclui criação de salas e histórico de mensagens.
     * Valor: `api/Chat`
     */
    const val CHAT_API = "$API/Chat"

    /**
     * Rota base para consulta das Tabelas de Classificação (Leaderboards).
     * Valor: `api/Leaderboard`
     */
    const val LEADBOARD_API = "$API/Leaderboard"

    /**
     * Rota base para gestão de dados de Jogadores.
     * Utilizada para consultar perfis, editar informações e pesquisas de mercado.
     * Valor: `api/Player`
     */
    const val PLAYER_API = "$API/Player"

    /**
     * Rota base para gestão de Equipas.
     * Inclui criação, edição, consulta de detalhes e gestão de membros da equipa.
     * Valor: `api/Team`
     */
    const val TEAM_API = "$API/Team"

    /**
     * Rota base para gestão de **Convites de Jogo (Match Invites)**.
     * Utilizada para enviar, aceitar, recusar e negociar desafios de jogos entre equipas.
     * Valor: `api/MatchInvite`
     */
    const val MATCH_INVITE_API = "$API/MatchInvite"

    const val POSTPONED_MATCH_API = TEAM_API

}

/**
 * Objeto utilitário que centraliza as constantes das chaves (Keys) de argumentos de navegação.
 *
 * Estas constantes são utilizadas para nomear os parâmetros nas rotas do Navigation Compose
 * e para extrair os valores correspondentes do `BackStackEntry` nos ecrãs de destino.
 *
 * O uso deste objeto evita erros de digitação ("Magic Strings") e garante a consistência
 * dos nomes dos parâmetros em toda a aplicação.
 */
object Arguments {

    /**
     * Chave utilizada para passar o Identificador Único (UUID) de uma **Equipa**.
     *
     * Usado em rotas como: Detalhes da Equipa, Edição de Equipa, Calendário da Equipa.
     */
    const val TEAM_ID = "teamId"

    /**
     * Chave utilizada para passar o Identificador Único (UUID) de uma **Equipa**.
     *
     * Alternativa ou redundância para [TEAM_ID].
     */
    const val ID_TEAM = "idTeam"

    /**
     * Chave utilizada para passar o Identificador Único (UUID) de um **Jogador**.
     *
     * Usado em rotas como: Perfil do Jogador, Chat Privado, Gestão de Membros.
     */
    const val PLAYER_ID = "playerId"

    /**
     * Chave utilizada para passar o Identificador Único de um **Jogo Agendado**.
     *
     * Usado em rotas como: Detalhes do Jogo, Resultado do Jogo.
     */
    const val MATCH_ID = "matchId"

    /**
     * Chave utilizada para passar o Identificador Único de um **Convite de Jogo**.
     *
     * Usado em rotas como: Negociação de Convite, Aceitar/Rejeitar Desafio.
     */
    const val MATCH_INVITE_ID = "matchInviteId"

    /**
     * Chave utilizada para passar o Identificador Único de um **Convite de Jogo**.
     *
     * Alternativa ou redundância para [MATCH_INVITE_ID].
     */
    const val ID_MATCH_INVITE = "idMatchInvite"

    /**
     * Chave utilizada para passar o Identificador Único de um **Pedido de Adesão** (Membership Request).
     *
     * Usado em rotas de gestão de pedidos de adesão de jogadores ou convites de equipa.
     */
    const val REQUEST_ID = "requestId"

    /**
     * Chave utilizada para passar o **Nome da Equipa**.
     *
     * Útil para exibir títulos ou informações contextuais na barra de topo, evitando
     * a necessidade de carregar o objeto completo da equipa imediatamente.
     */
    const val TEAM_NAME = "teamName"

    /**
     * Chave utilizada para definir o **Modo de Operação** de um formulário.
     *
     * Geralmente transporta o nome de um Enum (ex: "CREATE", "EDIT", "VIEW") ou um valor inteiro,
     * permitindo reutilizar o mesmo ecrã (Composable) para criar ou editar entidades.
     */
    const val FORM_MODE = "formMode"

    /**
     * Chave utilizada para definir o **Modo de Listagem de Jogadores** ([ListPlayerMode]).
     *
     * Determina se a lista deve exibir jogadores com equipa ou jogadores sem equipa (Free Agents).
     */
    const val LIST_PLAYER_MODE = "listPlayer"

    /**
     * Chave utilizada para definir o **Modo de Listagem de Equipas** ([ListTeamMode]).
     *
     * Determina o contexto da lista, como listar todas as equipas, equipas para convidar para jogo, etc.
     */
    const val LIST_TEAM_MODE = "listTeam"

    /**
     * Chave utilizada para definir o **Modo de Listagem de Pedidos de Adesão** ([ListMembershipRequestMode]).
     *
     * Determina se a lista deve exibir pedidos feitos por jogadores (para a equipa) ou convites feitos por equipas (para o jogador).
     */
    const val LIST_MEMBERSHIP_REQUEST_MODE = "listMemberShipRequest"
}

/**
 * Objeto utilitário que armazena constantes relacionadas à configuração de notificações do Android.
 *
 * Estas constantes são usadas para identificar canais de notificação (Notification Channels),
 * o que é essencial para o gerenciamento de notificações a partir do Android 8.0 (Oreo) e superior.
 */
object NotificationConst {
    /**
     * O ID único para o canal de notificação principal da equipa.
     *
     * Este canal é usado para notificações importantes relacionadas a mudanças no estado
     * da equipa, jogos, convites e gestão de membros.
     */
    const val TEAM_CHANNEL_ID = "team_channel_id"
}

object NetworkConsts {
    //"https://amfootballapi.duckdns.org/"
    const val BASE_URL = "https://thrillful-temika-postlicentiate.ngrok-free.dev/"
}

object SignalRUrls {
    const val START_MATCH_URL = "StartMatch"
}

object SignalRMethods {
    const val JOIN_MATCH = "JoinStartMatch"
    const val LEAVE_MATCH = "LeaveStartMatch"
    const val RECEIVE_MATCH = "ReceiveStartMatch"
}

object SignalRMessages {
    const val MATCH_STARTED = "O jogo começou!"
}