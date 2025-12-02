package com.example.amfootball.data.mocks.homePages

import com.example.amfootball.data.dtos.player.FireBaseLoginResponseDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.enums.Position

object HomePageMock {
    val mockLoginResponse = FireBaseLoginResponseDto(
        idToken = "mock_token_123",
        refreshToken = "mock_refresh_token",
        expiresIn = "3600",
        localId = "USER_ID_123",
        phoneNumber = 912345678,
        email = "tiago@test.com"
    )

    val mockUserNoTeam = PlayerProfileDto(
        loginResponseDto = mockLoginResponse,
        name = "Tiago Sem Equipa",
        email = "tiago@test.com",
        phoneNumber = "912345678",
        dateOfBirth = "2000-01-01",
        icon = null,
        address = "Rua Principal, Porto",
        position = Position.MIDFIELDER.name,
        height = 180,
        team = TeamDto(
            id = "",
            name = ""
        ),
        isAdmin = false
    )

    val mockUserWithTeam = PlayerProfileDto(
        loginResponseDto = mockLoginResponse,
        name = "Tiago Com Equipa",
        email = "tiago@test.com",
        phoneNumber = "912345678",
        dateOfBirth = "2000-01-01",
        icon = null,
        address = "Rua Principal, Lisboa",
        position = "Wide Receiver",
        height = 185,
        team = TeamDto(
            id = "TEAM_ID_123",
            name = "Porto Renegades"
        ),
        isAdmin = false
    )
}