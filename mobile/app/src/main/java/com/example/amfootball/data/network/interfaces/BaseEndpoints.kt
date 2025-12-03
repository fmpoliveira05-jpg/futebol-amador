package com.example.amfootball.data.network.interfaces

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
}