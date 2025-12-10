package com.example.amfootball.ui.previewsMocks

import com.example.amfootball.data.remote.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.remote.dtos.support.PitchInfo
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.data.remote.dtos.team.FormTeamDto
import com.example.amfootball.data.remote.dtos.team.ProfileTeamDto
import javax.inject.Singleton

@Singleton
object ListMatchInviteMocks {
    val mockItemsListActions =
        _root_ide_package_.com.example.amfootball.ui.actions.itemsList.ItemListMatchIniviteActions(
            acceptMatchInvite = {},
            rejectMatchInvite = {},
            negociateMatchInvite = { _, _ -> },
            showMoreDetails = { _, _ -> }
        )

    val mockList = listOf(
        MatchInviteDto(
            id = "1",
            opponent = TeamDto(
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
            opponent = TeamDto(
                id = "opp2",
                name = "Águias do Norte",
                image = ""
            ),
            namePitch = "Campo da Tapadinha",
            gameDateRaw = "2023-12-30T21:00:00",
            isHomeGame = false
        )
    )

    val mockFilterActions =
        _root_ide_package_.com.example.amfootball.ui.actions.filters.FilterMatchInviteActions(
            onSenderNameChange = {},
            onMinDateSelected = {},
            onMaxDateSelected = {},
            buttonActions = _root_ide_package_.com.example.amfootball.ui.actions.filters.ButtonFilterActions(
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

    val mockActions = _root_ide_package_.com.example.amfootball.ui.actions.forms.FormTeamActions(
        {}, {}, {}, {}, {}
    )

}