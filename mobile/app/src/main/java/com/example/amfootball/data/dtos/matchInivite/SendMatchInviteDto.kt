package com.example.amfootball.data.dtos.matchInivite

import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime

/**
 * Data Transfer Object (DTO) utilizado para enviar um novo convite de jogo para outra equipa.
 *
 * Este objeto encapsula as informações necessárias para iniciar uma negociação de partida,
 * definindo os intervenientes, a data proposta e o local.
 *
 * @property idSender O identificador único (UUID) da equipa que está a enviar o convite (a equipa desafiante).
 * @property idReceiver O identificador único (UUID) da equipa que irá receber o convite (a equipa adversária).
 * @property gameDate A data e hora propostas para a realização da partida.
 * @property homePitch Indica a proposta de local do jogo.
 * Se `true`, o jogo realiza-se no campo da equipa [idSender] (Casa).
 * Se `false`, o jogo realiza-se no campo da equipa [idReceiver] (Fora).
 */
data class SendMatchInviteDto(
    @SerializedName("IdMatch", alternate = ["idMatch"])
    var idMatch: String? = null,
    @SerializedName("IdSender", alternate = ["idSender", "idTeam"])
    val idSender: String,
    @SerializedName("IdReceiver", alternate = ["idReceiver"])
    val idReceiver: String,
    @SerializedName("IdOpponent", alternate = ["idOpponent"])
    val idOpponent: String = idReceiver,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val gameDate: String,
    @SerializedName("PostPoneDate", alternate = ["postPoneDate"])
    val postPoneDate: String = gameDate,
    @SerializedName("HomePitch", alternate = ["homePitch"])
    val homePitch: Boolean
)