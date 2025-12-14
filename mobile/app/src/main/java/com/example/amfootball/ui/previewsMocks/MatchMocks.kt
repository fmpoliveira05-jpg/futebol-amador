package com.example.amfootball.ui.previewsMocks

import com.example.amfootball.data.remote.dtos.match.ResultMatchDto
import com.example.amfootball.domains.errors.formErrors.FinishMatchFormErrors
import com.example.amfootball.ui.actions.forms.FormFinishMatchActions

object FinishMatchMocks {
    val mockResult = ResultMatchDto(
        idMatch = "match_123",
        idOpponent = "opponent_456",
        numGoalsTeam = 2,
        numGoalsOpponent = 1
    )

    val mockActions = FormFinishMatchActions(
        onNumGoalsTeamChange = {},
        onNumGoalsOpponentChange = {},
        onSubmitForm = {}
    )

    val mockErrors = FinishMatchFormErrors()
}