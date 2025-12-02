package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPlayersActions
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position

object ListPlayersMocks {
    val list = listOf(
        InfoPlayerDto(
            id = "1",
            name = "Lionel Messi",
            age = 36,
            address = "Miami, USA",
            heigth = 170,
            position = Position.FORWARD,
            haveTeam = true,
            image = ""
        ),
        InfoPlayerDto(
            id = "2",
            name = "Bernardo Silva",
            age = 29,
            address = "Manchester, UK",
            heigth = 173,
            position = Position.MIDFIELDER,
            haveTeam = false,
            image = ""
        )
    )

    val Actions = FilterListPlayersActions(
        onNameChange = {},
        onCityChange = {},
        onMinAgeChange = {},
        onMaxAgeChange = {},
        onPositionChange = {},
        onMinSizeChange = {},
        onMaxSizeChange = {},
        buttonActions = ButtonFilterActions(
            onFilterApply = {},
            onFilterClean = {}
        )
    )

    val Positions = listOf(null) + Position.values().toList()
}