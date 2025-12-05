package com.example.amfootball.data.dtos.match

/**
 * Data Transfer Object (DTO) utilizado para registar ou transmitir o resultado final de uma partida.
 *
 * Contém os identificadores da partida e das equipas, bem como o marcador final (golos).
 * É tipicamente usado no envio do formulário de finalização de jogo para a API.
 *
 * @property idMatch O identificador único da partida que está a ser finalizada.
 * @property idTeam O identificador único da equipa principal (geralmente a equipa do utilizador ou a equipa da casa).
 * @property numGoals O número de golos marcados pela equipa principal ([idTeam]). Padrão é 0.
 * @property idOpponent O identificador único da equipa adversária.
 * @property numGoalsOpponent O número de golos marcados pela equipa adversária ([idOpponent]). Padrão é 0.
 */
data class ResultMatchDto(
    val idMatch: String,
    val idTeam: String,
    val numGoals: Int = 0,
    val idOpponent: String,
    val numGoalsOpponent: Int = 0
)