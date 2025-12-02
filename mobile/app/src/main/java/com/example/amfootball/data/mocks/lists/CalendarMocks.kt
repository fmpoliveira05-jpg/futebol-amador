package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterCalendarActions
import com.example.amfootball.data.actions.itemsList.ItemsCalendarActions
import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.dtos.support.PitchInfo
import com.example.amfootball.data.dtos.support.TeamStatisticsDto
import java.time.LocalDateTime

object CalendarMocks {
    val filterActions = FilterCalendarActions(
        onNameChange = {},
        onMinDateGameChange = {},
        onMaxDateGameChange = {},
        onGameLocalChange = {},
        onTypeMatchChange = {},
        onFinishMatch = {},
        onButtonFilterActions = ButtonFilterActions(
            onFilterApply = {},
            onFilterClean = {}
        )
    )

    val itemActions = ItemsCalendarActions(
        onCancelMatch = { _, _ -> },
        onPostPoneMatch = { _, _ -> },
        onStartMatch = {},
        onFinishMatch = { _, _ -> }
    )

    val listNormal: List<InfoMatchCalendar>
        get() = listOf(
            InfoMatchCalendar(
                idMatch = "1",
                matchStatusId = 2, // DONE
                rawGameDate = LocalDateTime.now().minusDays(3).toString(),
                typeMatchBool = true,
                matchResultId = 0,
                pitchGame = PitchInfo("Estádio Municipal", "Rua Principal"),
                team = TeamStatisticsDto("t1", "Vitória SC", 3, ""),
                opponent = TeamStatisticsDto("t2", "Dragões FC", 1, ""),
                isHome = false
            ),
            InfoMatchCalendar(
                idMatch = "2",
                matchStatusId = 0, // SCHEDULED
                rawGameDate = LocalDateTime.now().plusDays(5).toString(),
                typeMatchBool = false,
                matchResultId = -1,
                pitchGame = PitchInfo("Campo de Treinos", "Avenida Secundária"),
                team = TeamStatisticsDto("t1", "Vitória SC", 0, ""),
                opponent = TeamStatisticsDto("t3", "Sporting CP", 0, ""),
                isHome = true
            )
        )
}