package com.example.amfootball.data.dtos.MembershipRequest

import android.net.Uri
import java.time.LocalDateTime

data class MembershipRequestInfoDto(
    val Id: String,
    val Receiver: String,
    val Sender: String,
    val ImageSender: Uri,
    val DateSend: LocalDateTime
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
                        Id = "t${i + 1}",
                        Receiver = teamName,
                        Sender = playerNames[i],
                        ImageSender = Uri.EMPTY,
                        DateSend = LocalDateTime.now().minusHours(i.toLong())
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
                        Id = "p${i + 1}",
                        Receiver = playerName,
                        Sender = teamNames[i],
                        ImageSender = Uri.EMPTY,
                        DateSend = LocalDateTime.now().minusDays(i.toLong())
                    )
                )
            }

            return list
        }
    }
}