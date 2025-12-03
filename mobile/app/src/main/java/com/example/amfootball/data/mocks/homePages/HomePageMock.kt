package com.example.amfootball.data.mocks.homePages

import com.example.amfootball.data.dtos.player.FireBaseLoginResponseDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.enums.Position

/**
 * Objeto utilitário que fornece dados fictícios (Mock Data) estáticos para o ecrã Principal (Home Page).
 *
 * Este objeto é essencial para o desenvolvimento de UI (Jetpack Compose Previews) e testes instrumentados,
 * permitindo visualizar o comportamento da interface sem necessidade de fazer chamadas reais à API ou Firebase.
 *
 * Cobre dois cenários principais de utilização:
 * 1. Utilizador novo sem equipa (Onboarding flow).
 * 2. Utilizador existente com equipa associada (Dashboard flow).
 */
object HomePageMock {

    /**
     * Simula uma resposta de autenticação bem-sucedida do Firebase.
     * Contém tokens e IDs fictícios necessários para preencher o objeto [PlayerProfileDto].
     */
    val mockLoginResponse = FireBaseLoginResponseDto(
        idToken = "mock_token_123",
        refreshToken = "mock_refresh_token",
        expiresIn = "3600",
        localId = "USER_ID_123",
        phoneNumber = 912345678,
        email = "tiago@test.com"
    )

    /**
     * Representa um perfil de utilizador que **ainda não pertence a nenhuma equipa**.
     *
     * Útil para testar o "Empty State" da Home Page, onde devem ser exibidos
     * botões para "Criar Equipa" ou "Procurar Equipa".
     *
     * Características:
     * - `team.id`: Vazio/Inválido.
     * - `team.name`: Vazio.
     */
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

    /**
     * Representa um perfil de utilizador que **já possui uma equipa associada** ("Porto Renegades").
     *
     * Útil para testar o estado normal da Home Page (Dashboard), onde são exibidas
     * estatísticas, próximos jogos e chat da equipa.
     *
     * Características:
     * - `team.id`: "TEAM_ID_123" (Válido).
     */
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