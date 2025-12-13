package com.example.amfootball.domains.enums.pages

/**
 * Define os modos de visualização disponíveis para a listagem de jogadores na aplicação.
 *
 * Utilizado para alternar entre a exibição de jogadores que pertencem a uma equipa
 * e a exibição de jogadores que estão atualmente sem equipa (disponíveis para convite).
 */
enum class ListPlayerMode {
    /**
     * Modo de visualização para listar **Jogadores que pertencem a uma Equipa**.
     *
     * Tipicamente usado para visualizar o plantel de uma equipa específica ou todos os membros da aplicação.
     */
    PLAYER_LIST,

    /**
     * Modo de visualização para listar **Jogadores que estão atualmente sem Equipa** (ou 'Free Agents').
     *
     * Usado por administradores de equipa para procurar e convidar novos membros.
     */
    PLAYER_WITHOU_TEAM
}