package com.example.amfootball.data.dtos.postponeMatch

import com.example.amfootball.data.dtos.support.TeamDto
import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime

/**
 * Objeto de Transferência de Dados (DTO) que representa os detalhes de um pedido de adiamento de jogo.
 *
 * Este objeto agrega toda a informação contextual necessária para avaliar uma remarcação,
 * cruzando os dados do jogo original com a nova proposta de data.
 *
 * A classe utiliza `LocalDateTime`, pressupondo que o conversor JSON (Gson) possui os TypeAdapters
 * necessários para serializar datas Java 8.
 *
 * @property idMatch O identificador único do jogo que está a ser adiado.
 * Mapeado de `"IdMatch"` ou `"idMatch"`.
 *
 * @property gameDate A data e hora **originalmente agendada** para o jogo.
 * Mapeado de `"GameDate"` ou `"gameDate"`.
 *
 * @property postponeDate A **nova data e hora proposta** para a realização do jogo.
 * Mapeado de `"PostPoneDate"` ou `"postPoneDate"`.
 *
 * @property team O objeto [TeamDto] representando a equipa principal no contexto (geralmente a equipa da casa ou a que iniciou o pedido).
 * Mapeado de `"Team"` ou `"team"`.
 *
 * @property opponent O objeto [TeamDto] representando a equipa adversária.
 * Mapeado de `"Opponent"` ou `"opponent"`.
 */
data class PostponeDto(
    @SerializedName("IdMatch", alternate = ["idMatch"])
    val idMatch: String,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val gameDate: LocalDateTime,
    @SerializedName("PostPoneDate", alternate = ["postPoneDate"])
    val postponeDate: LocalDateTime,
    @SerializedName("Team", alternate = ["team"])
    val team: TeamDto,
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamDto,
)