package com.example.amfootball.data.dtos.support

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa as estatísticas de uma Equipa dentro do contexto de uma única Partida.
 *
 * Utilizado para transmitir o resultado e a pontuação de uma equipa em relação a um jogo específico.
 *
 * @property infoTeam O objeto [TeamDto] que contém o ID e o nome da equipa.
 * @property numGoals O número de golos marcados pela equipa na partida. Padrão é 0.
 */
data class TeamStatisticsDto(
    @SerializedName("idTeam")
    val idTeam: String,
    @SerializedName("name")
    val name: String,
    val numGoals: Int = 0,
    val image: String? = null
)
