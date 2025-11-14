package com.example.amfootball.data.dtos.matchInivite

import android.net.Uri

data class InfoMatchInviteDto(
    val id: String = "",
    val idOpponent: String = "",
    val nameOpponent: String = "",
    val logoOpponent: Uri = Uri.EMPTY,
    val gameDate: String = "",
    val pitchGame: String = ""
) {
    companion object {
        fun generatePreviewList(): List<InfoMatchInviteDto> {
            val opponentNames = listOf(
                "Gondomar Vikings",
                "Lisboa Eagles",
                "Porto Lions",
                "Braga Bears",
                "Faro Saints"
            )
            val pitches = listOf("Campo Principal", "Estádio Municipal", "Centro de Treinos")

            return (1..20).map { i ->
                InfoMatchInviteDto(
                    id = "invite_$i",
                    idOpponent = "opponent_$i",
                    nameOpponent = opponentNames[i % opponentNames.size],
                    logoOpponent = Uri.EMPTY,
                    gameDate = "${10 + (i % 20)}/11/2025",
                    pitchGame = pitches[i % pitches.size]
                )
            }
        }
    }
}
