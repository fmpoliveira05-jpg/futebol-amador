package com.example.amfootball.data.dtos.matchInivite

import android.net.Uri
import com.example.amfootball.data.dtos.support.TeamDto
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa as informações essenciais de um convite de partida.
 *
 * Este objeto é utilizado para listar convites recebidos ou enviados, apresentando
 * os detalhes do adversário, a data proposta e o local.
 *
 * @property id O identificador único do convite.
 * @property opponent O objeto [TeamDto] com os dados da equipa adversária (Id, Nome, Imagem).
 * @property gameDate A data proposta para o jogo (armazenada como String para exibição direta).
 * @property pitchGame O nome do campo ou local onde o jogo está proposto acontecer.
 */
data class InfoMatchInviteDto(
    @SerializedName("Id", alternate = ["id"])
    val id: String = "",
    @SerializedName("Receiver", alternate = ["receiver"])
    val opponent: TeamDto,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val gameDate: String = "",
    @SerializedName("PitchGame", alternate = ["pitchGame"])
    val pitchGame: String = "",
    @SerializedName("IsHome", alternate = ["isHome"])
    val isHomeGame: Boolean = false
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
                    // CORREÇÃO: Criar o objeto TeamDto em vez de passar strings soltas
                    opponent = TeamDto(
                        id = "opponent_$i",
                        name = opponentNames[i % opponentNames.size],
                        image = null
                    ),
                    gameDate = "${10 + (i % 20)}/11/2025",
                    pitchGame = pitches[i % pitches.size]
                )
            }
        }
    }
}
