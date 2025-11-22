package com.example.amfootball.data.dtos.matchInivite

import android.net.Uri

/**
 * Data Transfer Object (DTO) que representa as informações essenciais de um convite de partida.
 *
 * Este objeto é utilizado para listar convites recebidos ou enviados, apresentando
 * os detalhes do adversário, a data proposta e o local.
 *
 * @property id O identificador único do convite.
 * @property idOpponent O identificador da equipa adversária (quem convida ou é convidado).
 * @property nameOpponent O nome da equipa adversária para exibição.
 * @property logoOpponent A URI do logótipo da equipa adversária.
 * @property gameDate A data proposta para o jogo (armazenada como String para exibição direta).
 * @property pitchGame O nome do campo ou local onde o jogo está proposto acontecer.
 */
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
