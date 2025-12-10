package com.example.amfootball.data.remote.dtos.match

import com.example.amfootball.data.remote.dtos.support.TeamDto
import java.time.LocalDateTime

/**
 * Data Transfer Object (DTO) que representa os detalhes de uma partida de futebol adiada.
 *
 * É usado para transportar dados sobre o jogo original e sua subsequente remarcação.
 *
 * @property id Identificador único (String, geralmente um UUID) da partida adiada.
 * @property opponent O objeto [TeamDto] que representa a equipa adversária.
 * @property gameDate A data e hora originais em que a partida deveria ter ocorrido.
 * @property postPoneDate A nova data e hora em que a partida foi remarcada após o adiamento.
 * @property pitchMatch O nome do campo ou estádio onde a partida será realizada.
 */
data class PostPoneMatchDto(
    val id: String,
    val opponent: TeamDto,
    val gameDate: LocalDateTime,
    val postPoneDate: LocalDateTime,
    val pitchMatch: String
)