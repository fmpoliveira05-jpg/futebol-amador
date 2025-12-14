package com.example.amfootball.data.remote.dtos.membershipRequest

import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

/**
 * Data Transfer Object (DTO) que representa os detalhes de um pedido de adesão (membership request).
 *
 * Contém informações sobre quem enviou e quem recebeu o pedido, a data e o contexto (se foi iniciado pelo jogador ou pela equipa).
 *
 * @property id O identificador único do pedido.
 * @property player Informações sobre o jogador do pedido.
 * @property team Informações sobre a equipa do pedido.
 * @property dateSend A data e hora em que o pedido foi enviado.
 * @property isPlayerSender Indica se o pedido foi enviado pelo jogador (true) ou pela equipa (false).
 */
data class MembershipRequestInfoDto(
    @SerializedName("RequestId", alternate = ["requestId"])
    val id: String,
    @SerializedName("Player", alternate = ["player"])
    val player: PlayerDto,
    @SerializedName("Team", alternate = ["team"])
    val team: TeamDto,
    @SerializedName("RequestDate", alternate = ["requestDate"])
    val dateSend: String,
    @SerializedName("IsPlayerSender", alternate = ["isPlayerSender"])
    val isPlayerSender: Boolean
) {
    val requestDate: LocalDateTime
        get() {
            return LocalDateTime.parse(dateSend, DateTimeFormatter.ISO_DATE_TIME)
        }
}