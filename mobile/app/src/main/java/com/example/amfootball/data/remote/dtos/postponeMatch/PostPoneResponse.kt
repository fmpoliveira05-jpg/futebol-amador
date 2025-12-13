package com.example.amfootball.data.remote.dtos.postponeMatch

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) utilizado para enviar a resposta (aceitação ou rejeição)
 * a um pedido de adiamento de jogo (Postpone Match) para a API.
 *
 * Esta classe é usada em conjunto com as funções de aceitação e rejeição no [PostPoneMatchService].
 *
 * @property idMatch O identificador único do jogo que está a ser objeto da solicitação de adiamento.
 * @property statusPostPone O novo estado desejado para a solicitação de adiamento (ex: 1 para ACEITAR, 2 para REJEITAR).
 * É mapeado a partir do campo JSON "StatusPostPone".
 * @property idTeam O identificador da equipa que está a responder à solicitação (a equipa que aceita ou rejeita).
 * @property idOpponent O identificador da equipa adversária (ou a equipa que enviou o pedido original, dependendo do contexto da API).
 */
data class PostPoneResponse(
    @SerializedName("IdMatch")
    val idMatch: String,
    @SerializedName("StatusPostPone")
    val statusPostPone: Int,
    @SerializedName("IdTeam")
    val idTeam: String,
    @SerializedName("IdOpponent")
    val idOpponent: String
)