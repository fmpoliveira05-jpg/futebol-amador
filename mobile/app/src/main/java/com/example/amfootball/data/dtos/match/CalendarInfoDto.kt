package com.example.amfootball.data.dtos.match

import com.example.amfootball.data.dtos.support.TeamStatisticsDto
import com.example.amfootball.data.enums.TypeMatch
import java.time.LocalDate

/**
 * Data Transfer Object (DTO) que representa as informações resumidas de uma partida
 * para visualização num calendário ou lista de histórico.
 *
 * Agrega informações sobre as duas equipas envolvidas, o resultado (se houver),
 * o tipo de jogo, a localização (Casa/Fora) e a data.
 *
 * @property idMatch O identificador único da partida.
 * @property team As estatísticas e informações da equipa principal (ou equipa do utilizador) neste contexto.
 * @property opponent As estatísticas e informações da equipa adversária.
 * @property typeMatch O tipo de jogo (ex: [TypeMatch.COMPETITIVE] ou [TypeMatch.CASUAL]).
 * @property gameLocale Indica o local do jogo. Se `true`, é um jogo em **Casa** (Home); se `false`, é um jogo **Fora** (Away).
 * @property matchDate A data em que a partida ocorreu ou está agendada para ocorrer.
 */
data class CalendarInfoDto(
    val idMatch: String,
    val team: TeamStatisticsDto,
    val opponent: TeamStatisticsDto,
    val typeMatch: TypeMatch,
    val gameLocale: Boolean,
    val matchDate: LocalDate,
    val isFinish: Boolean
)