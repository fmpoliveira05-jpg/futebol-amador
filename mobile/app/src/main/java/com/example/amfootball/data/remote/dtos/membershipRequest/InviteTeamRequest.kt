package com.example.amfootball.data.remote.dtos.membershipRequest

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) utilizado para enviar uma solicitação que identifica uma equipa.
 *
 * É tipicamente usado para convidar ou solicitar alguma ação relacionada a uma equipa específica.
 *
 * @property teamId O identificador único da equipa alvo da requisição. Mapeado a partir do campo JSON "TeamId".
 */
data class InviteTeamRequest(
    @SerializedName("TeamId")
    val teamId: String
)