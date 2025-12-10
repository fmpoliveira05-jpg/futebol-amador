package com.example.amfootball.data.mocks

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMatchInviteActions
import com.example.amfootball.data.actions.forms.FormTeamActions
import com.example.amfootball.data.actions.itemsList.ItemListMatchIniviteActions
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.dtos.support.PitchInfo
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import javax.inject.Singleton

@Singleton
object ListMatchInviteMocks {
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

object ProfileTeamMocks {
    val dummyPitch = PitchInfo(
        name = "Estádio D. Afonso Henriques",
        address = "Guimarães",
    )

    val dummyTeam = ProfileTeamDto(
        id = "1",
        name = "Vitória SC",
        description = "O Conquistador. Clube histórico de Portugal.",
        foundationDate = "1922",
        rank = "Ouro",
        totalPoints = 350,
        logo = "",
        pitch = dummyPitch,
    )
}

object EditTeamMocks {
    val mockEditTeam = FormTeamDto(
        name = "Vitória SC",
        description = "Os Conquistadores. A maior equipa do Minho.",
        pitch = PitchInfo(
            name = "Estádio D. Afonso Henriques",
            address = "Praça 26 de Maio, Guimarães"
        )
    )

    val mockActions = FormTeamActions(
        {}, {}, {}, {}, {}
    )

}