package com.example.amfootball.domains.enums.pages

/**
 * Define os modos de visualização disponíveis para a listagem de equipas na aplicação.
 *
 * Este enum é utilizado para contextuar a finalidade da lista de equipas apresentada ao utilizador,
 * influenciando quais ações são permitidas sobre os itens da lista.
 */
enum class ListTeamMode {
    /**
     * Modo de visualização genérico para listar **Todas as Equipa** registadas na aplicação.
     *
     * Tipicamente usado para navegação geral ou visualização de perfis de equipas.
     */
    LIST_TEAM,

    /**
     * Modo de visualização usado para listar equipas que podem ser **convidadas para um Jogo** (Match Invite).
     *
     * Opcionalmente, pode aplicar filtros para mostrar apenas equipas disponíveis ou elegíveis.
     */
    LIST_TEAM_MATCH_INVITE,

    /**
     * Modo de visualização usado para listar equipas para as quais um jogador pode **solicitar Adesão** (Membership Request).
     *
     * Opcionalmente, pode aplicar filtros para excluir equipas onde o jogador já tem um pedido pendente ou é membro.
     */
    LIST_TEAM_MEMBERSHIP_REQUEST
}