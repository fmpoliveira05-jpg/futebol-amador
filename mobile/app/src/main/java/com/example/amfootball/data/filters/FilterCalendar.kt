package com.example.amfootball.data.filters

import com.example.amfootball.data.enums.TypeMatch
import java.time.LocalDateTime

/**
 * Data class que contém todos os critérios de filtro aplicáveis a uma lista de partidas (calendário).
 *
 * Utilizado para persistir e transferir o estado de filtragem entre a UI e a camada de dados/lógica.
 * Todos os campos são opcionais, permitindo filtros parciais.
 *
 * @property opponentName Filtro pelo nome da equipa adversária.
 * @property minGameDate Filtro pela data e hora mínima de realização da partida.
 * @property maxGameDate Filtro pela data e hora máxima de realização da partida.
 * @property gameLocale Filtro por localização: true (Casa), false (Fora), ou null (Indiferente).
 * @property typeMatch Filtro pelo tipo de partida ([TypeMatch]), ou null (Indiferente).
 * @property isFinish Filtro pelo estado final da partida: true (Finalizada), false (Não Finalizada), ou null (Indiferente).
 */
data class FilterCalendar(
    val opponentName: String? = null,
    val minGameDate: LocalDateTime? = null,
    val maxGameDate: LocalDateTime? = null,
    val gameLocale: Boolean? = null,
    val typeMatch: TypeMatch? = null,
    val isFinish: Boolean? = null,
)