package com.example.amfootball.ui.previewsMocks

import com.example.amfootball.data.remote.dtos.player.FireBaseLoginResponseDto
import com.example.amfootball.data.remote.dtos.player.LoginDto
import com.example.amfootball.data.remote.dtos.player.PlayerProfileDto
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.ui.actions.forms.LoginActions

object ProfileUserMock {
    val dummyLoginResponse = FireBaseLoginResponseDto(
        idToken = "token_mock",
        refreshToken = "refresh_mock",
        expiresIn = "3600",
        localId = "1",
        phoneNumber = 934567890,
        email = "lm10@email.com"
    )

    val dummyProfile = PlayerProfileDto(
        loginResponseDto = dummyLoginResponse,
        name = "Lionel Messi",
        dateOfBirth = "24/06/1987",
        icon = null,
        address = "Rosario, Argentina",
        positionRaw = 3,
        height = 170,
        team = TeamDto(
            id = "2",
            name = "Inter Miami CF",
            image = ""
        ),
        isAdmin = false,
        email = dummyLoginResponse.email,
        phoneNumber = dummyLoginResponse.phoneNumber.toString()
    )
}

object LoginMocks {
    val login = LoginDto(
        email = "test@example.com",
        password = ""
    )

    val loginActions = LoginActions(
        onLoginUser = { _, onResult ->
            onResult(true)
        },
        onEmailChange = {},
        onPasswordChange = {},
        onIsUserLoggedInChange = {}
    )
}