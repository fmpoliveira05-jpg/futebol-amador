package com.example.amfootball.data.filters

import java.time.LocalDateTime

/**
 * Data class que contém todos os critérios de filtro aplicáveis a uma lista de partidas adiadas (postponed matches).
 *
 * Utilizado para persistir e transferir o estado de filtragem, permitindo pesquisar por dados originais
 * do jogo e por dados da nova data de remarcação.
 *
 * @property nameOpponent Filtro pelo nome da equipa adversária.
 * @property isHome Filtro por localização: true (Casa), false (Fora), ou null (Indiferente).
 * @property minDataGame Filtro pela data e hora mínima original do jogo.
 * @property maxDateGame Filtro pela data e hora máxima original do jogo.
 * @property minDatePostPone Filtro pela data e hora mínima de adiamento/remarcação.
 * @property maxDatePostPone Filtro pela data e hora máxima de adiamento/remarcação.
 */
data class FilterPostPoneMatch(
    val nameOpponent: String? = null,
    val isHome: Boolean? = null,
    val minDataGame: LocalDateTime? = null,
    val maxDateGame: LocalDateTime? = null,
    val minDatePostPone: LocalDateTime? = null,
    val maxDatePostPone: LocalDateTime? = null
)