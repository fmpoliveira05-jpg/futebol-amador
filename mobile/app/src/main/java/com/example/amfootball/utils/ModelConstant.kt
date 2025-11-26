package com.example.amfootball.utils

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

    /** Tamanho fixo esperado para um número de telemóvel (incluindo indicativo). */
    const val SIZE_PHONE_NUMBER = 13

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
    const val MIN_CANCEL_REASON_LENGTH = 50

    /** Tamanho Máximo da mensagem do Motivo de Cancelamento da Partida*/
    const val MAX_CANCEL_REASON_LENGTH = 5


}