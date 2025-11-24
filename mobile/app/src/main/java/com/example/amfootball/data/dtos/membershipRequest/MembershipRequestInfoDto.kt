package com.example.amfootball.data.dtos.membershipRequest

import android.net.Uri
import java.time.LocalDateTime

/**
 * Data Transfer Object (DTO) que representa os detalhes de um pedido de adesão (membership request).
 *
 * Contém informações sobre quem enviou e quem recebeu o pedido, a data e o contexto (se foi iniciado pelo jogador ou pela equipa).
 *
 * @property id O identificador único do pedido.
 * @property receiver Informações sobre a entidade recetora do pedido.
 * @property sender Informações sobre a entidade que enviou o pedido.
 * @property dateSend A data e hora em que o pedido foi enviado.
 * @property isPlayerSender Indica se o pedido foi enviado pelo jogador (true) ou pela equipa (false).
 */
data class MembershipRequestInfoDto(
    val id: String,
    val receiver: ReceiverInfo,
    val sender: SenderInfo,
    val dateSend: LocalDateTime,
    val isPlayerSender: Boolean
) {
    companion object {
        fun generateMemberShipRequestTeam(): List<MembershipRequestInfoDto> {
            val list = ArrayList<MembershipRequestInfoDto>()
            val teamName = "Vitoria SC"

            val playerNames = listOf(
                "Joao da Esquina", "Joaquim Alberto", "Anthony", "Jota Silva", "Zequinha da Farmacia",
                "Manuel Ribeiro", "Carlos Martins", "Bruna Alves", "Pedro Sousa", "Ana Ferreira",
                "Ricardo Costa", "Marta Silva", "Tiago Gomes", "Sofia Lopes", "André Santos",
                "Beatriz Pinto", "Miguel Fernandes", "Catarina Dias", "Rui Andrade", "Daniela Moreira"
            )

            for (i in 0 until 20) {
                list.add(
                    MembershipRequestInfoDto(
                        id = "r${i + 1}",
                        receiver = ReceiverInfo(
                            id = "t${i + 1}",
                            name = teamName
                        ),
                        sender = SenderInfo(
                            id = "p${i + 1}",
                            name = playerNames[i],
                            image = Uri.EMPTY
                        ),
                        dateSend = LocalDateTime.now().plusDays(i.toLong()),
                        isPlayerSender = false,
                    )
                )
            }

            return list
        }

        fun generateMemberShipRequestPlayer(): List<MembershipRequestInfoDto> {
            val list = ArrayList<MembershipRequestInfoDto>()
            val playerName = "Artur Jorge" // O receptor é sempre o jogador

            val teamNames = listOf(
                "SL Benfica", "FC Porto", "Sporting CP", "SC Braga", "Manchester City",
                "Real Madrid", "FC Barcelona", "Bayern Munich", "Liverpool FC", "Paris Saint-Germain",
                "Juventus", "AC Milan", "Inter Milan", "Arsenal", "Chelsea",
                "Atletico Madrid", "Borussia Dortmund", "Ajax", "Sevilla FC", "Gil Vicente"
            )

            for (i in 0 until 20) {
                list.add(
                    MembershipRequestInfoDto(
                        id = "r${i + 1}",
                        receiver = ReceiverInfo(
                            id = "t${i + 1}",
                            name = teamNames[i]
                        ),
                        sender = SenderInfo(
                            id = "p${i + 1}",
                            name = playerName,
                            image = Uri.EMPTY
                        ),
                        dateSend = LocalDateTime.now().plusDays(i.toLong()),
                        isPlayerSender = true,
                    )
                )
            }

            return list
        }
    }
}