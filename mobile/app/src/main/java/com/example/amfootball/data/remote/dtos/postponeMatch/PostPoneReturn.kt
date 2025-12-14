package com.example.amfootball.data.remote.dtos.postponeMatch

import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime

/**
 * Data Transfer Object (DTO) que representa o resultado retornado pela API após
 * uma solicitação bem-sucedida de adiamento (Postpone) de uma partida.
 *
 * Este DTO contém a informação chave do jogo que foi adiado, incluindo a nova data/hora
 * e os detalhes das equipas envolvidas.
 *
 * @property idMatch O identificador único da partida que foi adiada.
 * @property gameDate A nova data e hora do jogo (após o adiamento).
 * @property nameTeam O nome da equipa que solicitou o adiamento (ou a equipa anfitriã).
 * @property nameOpponent O nome da equipa adversária.
 * @property namePitch O nome do campo ou local onde a partida será realizada.
 */
data class PostPoneReturn(
    @SerializedName("IdMatch")
    val idMatch: String,
    @SerializedName("StatusPostPone")
    val gameDate: LocalDateTime,
    @SerializedName("NameTeam")
    val nameTeam: String,
    @SerializedName("NameOpponent")
    val nameOpponent: String,
    @SerializedName("NamePitch")
    val namePitch: String,
)
