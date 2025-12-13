package com.example.amfootball.domains.enums.pages

/**
 * Define os modos possíveis para a listagem de pedidos de adesão (Membership Requests).
 *
 * Utilizado para distinguir se a visualização atual deve focar nos pedidos
 * feitos por jogadores a uma equipa ou nas solicitações feitas por uma equipa a um jogador.
 */
enum class ListMembershipRequestMode {
    /**
     * Modo de visualização focado nos pedidos de adesão **feitos por Jogadores** a uma Equipa.
     *
     * Tipicamente usado pelo administrador de uma equipa para ver quem pediu para entrar.
     */
    MEMBERSHIP_PLAYER,

    /**
     * Modo de visualização focado nas solicitações de adesão **feitas por uma Equipa** a um Jogador.
     *
     * Tipicamente usado pelo jogador para ver quais equipas o convidaram.
     */
    MEMBERSHIP_TEAM
}