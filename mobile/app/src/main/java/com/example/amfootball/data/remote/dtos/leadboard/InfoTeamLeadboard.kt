package com.example.amfootball.data.remote.dtos.leadboard

import android.net.Uri
import com.google.gson.annotations.SerializedName

/**
 * DTO que representa as informações resumidas de uma equipa na Tabela de Classificação (Leaderboard).
 *
 * Contém os dados essenciais para exibir uma linha na tabela de ranking.
 *
 * @property id O identificador único da equipa.
 * @property name O nome da equipa.
 * @property currentPoints A pontuação atual acumulada pela equipa.
 * @property nameRank O nome ou rótulo do rank/nível da equipa (ex: "Ouro", "Elite", "1º Divisão").
 * @property logoTeam A URI do logótipo da equipa (padrão: [Uri.EMPTY]).
 */
data class InfoTeamLeadboard(
    @SerializedName("id", alternate = ["Id"])
    val id: String,
    @SerializedName("position", alternate = ["Position"])
    val position: Int,
    @SerializedName("teamName", alternate = ["TeamName"])
    val name: String,
    @SerializedName("currentPoints", alternate = ["CurrentPoints"])
    val currentPoints: Int,
    @SerializedName("rankName", alternate = ["RankName"])
    val nameRank: String,
    val logoTeam: String? = null
)