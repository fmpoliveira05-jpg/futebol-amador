package com.example.amfootball.data.mocks

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMatchInviteActions
import com.example.amfootball.data.actions.itemsList.ItemListMatchIniviteActions
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import javax.inject.Singleton

@Singleton
object listMatchInviteMocks {
    val mockItemsListActions = ItemListMatchIniviteActions(
        acceptMatchInvite = {},
        rejectMatchInvite = {},
        negociateMatchInvite = { _, _ -> },
        showMoreDetails = { _, _ -> }
    )

    val mockList = listOf(
        MatchInviteDto(
            id = "1",
            opponent = com.example.amfootball.data.dtos.support.TeamDto(
                id = "opp1",
                name = "Dragões de Leiria",
                image = ""
            ),
            namePitch = "Estádio Municipal",
            gameDateRaw = "2023-12-25T15:30:00",
            isHomeGame = true
        ),
        MatchInviteDto(
            id = "2",
            opponent = com.example.amfootball.data.dtos.support.TeamDto(
                id = "opp2",
                name = "Águias do Norte",
                image = ""
            ),
            namePitch = "Campo da Tapadinha",
            gameDateRaw = "2023-12-30T21:00:00",
            isHomeGame = false
        )
    )

    val mockFilterActions = FilterMatchInviteActions(
        onSenderNameChange = {},
        onMinDateSelected = {},
        onMaxDateSelected = {},
        buttonActions = ButtonFilterActions(
            onFilterApply = {},
            onFilterClean = {}
        )
    )
}