package com.example.amfootball.navigation.objects

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
     * Chave utilizada para definir o **Modo de Operação** de um formulário.
     *
     * Geralmente transporta o nome de um Enum (ex: "CREATE", "EDIT", "VIEW") ou um valor inteiro,
     * permitindo reutilizar o mesmo ecrã (Composable) para criar ou editar entidades.
     */
    const val FORM_MODE = "formMode"
    const val TEAM_NAME = "teamName"
}