package com.example.amfootball.data.dtos.match

import android.net.Uri
import com.example.amfootball.data.dtos.suporrtDto.TeamDto
import com.example.amfootball.data.dtos.suporrtDto.TeamStatisticsDto
import com.example.amfootball.data.enums.MatchResult
import com.example.amfootball.data.enums.TypeMatch
import java.time.LocalDate
import java.time.LocalDateTime
import java.util.UUID

data class CalendarInfoDto(
    val idMatch: String,
    val team: TeamStatisticsDto,
    val opponent: TeamStatisticsDto,
    val typeMatch: TypeMatch,
    val matchDate: LocalDate
) {
    companion object {
        fun createExampleCalendarList(): List<CalendarInfoDto> {
            val teamNames = listOf(
                "Águias de Lisboa", "Dragões do Porto", "Leões de Alvalade",
                "Guerreiros de Braga", "Panteras da Boavista", "Canarinhos de Viseu",
                "Lobos de Famalicão", "Castores de Paços", "Gilistas de Barcelos", "Académicos de Coimbra"
            )

            val allMatchResults = MatchResult.values()
            val now = LocalDateTime.now()

            return (1..50).map { index ->

                val team1Dto = TeamDto(
                    id = UUID.randomUUID().toString(),
                    name = teamNames.random(),
                    image = Uri.EMPTY
                )

                val team2Dto = TeamDto(
                    id = UUID.randomUUID().toString(),
                    name = teamNames.random(),
                    image = Uri.EMPTY
                )

                val resultTeam1 = allMatchResults.random()
                val resultTeam2: MatchResult
                val goalsTeam1: Int
                val goalsTeam2: Int

                when (resultTeam1) {
                    MatchResult.WIN -> {
                        goalsTeam1 = (1..5).random()
                        goalsTeam2 = (0 until goalsTeam1).random()
                        resultTeam2 = MatchResult.LOSE
                    }
                    MatchResult.LOSE -> {
                        goalsTeam2 = (1..5).random()
                        goalsTeam1 = (0 until goalsTeam2).random()
                        resultTeam2 = MatchResult.WIN
                    }
                    MatchResult.DRAW -> {
                        goalsTeam1 = (0..4).random()
                        goalsTeam2 = goalsTeam1
                        resultTeam2 = MatchResult.DRAW
                    }
                    MatchResult.UNDEFINED -> {
                        goalsTeam1 = 0
                        goalsTeam2 = 0
                        resultTeam2 = MatchResult.UNDEFINED
                    }
                }

                // 3. Cria as estatísticas de cada equipa
                val team1Stats = TeamStatisticsDto(
                    id = UUID.randomUUID().toString(),
                    infoTeam = team1Dto,
                    numGoals = goalsTeam1,
                    matchResult = resultTeam1
                )

                val team2Stats = TeamStatisticsDto(
                    id = UUID.randomUUID().toString(),
                    infoTeam = team2Dto,
                    numGoals = goalsTeam2,
                    matchResult = resultTeam2
                )

                val now = LocalDate.now()
                val matchDate = now.plusDays(index.toLong() - 25)

                CalendarInfoDto(
                    idMatch = UUID.randomUUID().toString(),
                    team = team1Stats,
                    opponent = team2Stats,
                    typeMatch = listOf(TypeMatch.CASUAL, TypeMatch.COMPETITIVE).random(), // Aleatoriamente Casual ou Competitivo
                    matchDate = matchDate
                )
            }
        }
    }
}
