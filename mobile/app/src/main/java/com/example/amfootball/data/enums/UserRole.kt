package com.example.amfootball.data.enums

/**
 * Define o nível de permissão e o estado atual do utilizador na aplicação.
 */
enum class UserRole {
    /**
     * O utilizador não está autenticado (Login falhou ou tokens nulos).
     */
    UNAUTHORIZED,

    /**
     * O utilizador está autenticado, mas não pertence a nenhuma equipa.
     */
    PLAYER_WITHOUT_TEAM,

    /**
     * O utilizador pertence a uma equipa como membro regular (Jogador/Staff).
     */
    MEMBER_TEAM,

    /**
     * O utilizador pertence a uma equipa e tem permissões de gestão (Capitão/Admin).
     */
    ADMIN_TEAM
}