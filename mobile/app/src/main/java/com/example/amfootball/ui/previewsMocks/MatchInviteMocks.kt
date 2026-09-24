package com.example.amfootball.ui.previewsMocks

import com.example.amfootball.data.remote.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.ui.actions.forms.FormMatchInviteActions

object FormMatchInviteMock {
    val emptyFields = MatchInviteDto(
        opponent = TeamDto(
            id = "1",
            name = ""
        ),
        gameDateString = "",
        gameTimeString = null,
        isHomeGame = true
    )

    val filledFields = MatchInviteDto(
        opponent = TeamDto(
            id = "2",
            name = "Lisboa Navigators"
        ),
        gameDateString = "2024-12-25",
        gameTimeString = "15:30",
        isHomeGame = true
    )

    val dummyActions = FormMatchInviteActions(
        onGameDateChange = {},
        onTimeGameChange = {},
        onLocalGameChange = {},
        onSubmitForm = { _ -> },
        onCancelForm = { _, _ -> }
    )
}