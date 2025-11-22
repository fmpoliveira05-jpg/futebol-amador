package com.example.amfootball.data.dtos.support

import com.example.amfootball.data.enums.MatchResult

/**
 * Data Transfer Object (DTO) que representa as estatísticas de uma Equipa dentro do contexto de uma única Partida.
 *
 * Utilizado para transmitir o resultado e a pontuação de uma equipa em relação a um jogo específico.
 *
 * @property id O identificador único desta entrada de estatística.
 * @property infoTeam O objeto [TeamDto] que contém o ID e o nome da equipa.
 * @property numGoals O número de golos marcados pela equipa na partida. Padrão é 0.
 * @property matchResult O resultado da equipa no jogo (WIN, LOSE, DRAW, UNDEFINED). Padrão é [MatchResult.UNDEFINED].
 */
data class TeamStatisticsDto(
    val id: String,
    val infoTeam: TeamDto,
    val numGoals: Int = 0,
    val matchResult: MatchResult = MatchResult.UNDEFINED,
)
