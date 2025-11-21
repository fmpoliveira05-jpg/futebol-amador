package com.example.amfootball.data.dtos.team

import com.example.amfootball.data.dtos.support.PitchInfo
import com.google.gson.annotations.SerializedName

/**
 * Data class com toda a informação que deve aparecer no perfil da equipa
 * */
data class ProfileTeamDto(
    val id: String = "",
    val name: String = "",
    val description: String = "",
    val foundationDate: String? = null,
    val totalPoints: Int,
    val rank: String,
    val logo: String? = null,
    @SerializedName("PitchDto")
    val pitch: PitchInfo = PitchInfo(),
)
{
    companion object {
        fun profileExempleTeam(): ProfileTeamDto {
            return ProfileTeamDto(
                id = "team_demo_001",
                name = "Os Invencíveis FC",
                description = "Equipa de exemplo.",
                foundationDate = "2024-01-01",
                totalPoints = 42,
                rank = "Ouro",
                pitch = PitchInfo(
                    name = "Estádio Demo",
                    address = "Rua dos Testes, Nº 10"
                ),
                logo = null
            )
        }
    }
}