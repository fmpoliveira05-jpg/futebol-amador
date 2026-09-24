package com.example.amfootball.data.remote.dtos.match

import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime

data class InfoMatch(
    val idMatch: String,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val rawGameDate: String = "",
    @SerializedName("IsCompetitive", alternate = ["isCompetitive"])
    val typeMatchBool: Boolean = false,
    @SerializedName("Team", alternate = ["team"])
    val team: TeamDto,
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamDto,
    @SerializedName("IsHome", alternate = ["isHome"])
    val isHome: Boolean = false,
) {
    /**
     * Converte a String bruta da API para um objeto [LocalDateTime].
     * Caso falhe a conversão (ex: string vazia ou formato inválido), retorna a data/hora atual.
     */
    val gameDate: LocalDateTime
        get() = try {
            LocalDateTime.parse(rawGameDate)
        } catch (e: Exception) {
            LocalDateTime.now()
        }
}