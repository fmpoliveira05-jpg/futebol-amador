package com.example.amfootball.data.remote.dtos.membershipRequest

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) utilizado para realizar uma solicitação de convite ou
 * outra ação direcionada a um jogador específico, identificando-o pelo seu ID.
 *
 * A propriedade [playerId] utiliza o campo JSON "PlayerId" como primário, mas aceita "playerId"
 * como um nome alternativo, garantindo maior flexibilidade no mapeamento de JSON.
 *
 * @property playerId O identificador único do jogador alvo da requisição (ex: jogador a ser convidado para uma equipa).
 * Mapeado a partir do campo JSON "PlayerId" (ou "playerId").
 */
data class InvitePlayerRequest(
    @SerializedName("PlayerId", alternate = ["playerId"])
    val playerId: String
)
