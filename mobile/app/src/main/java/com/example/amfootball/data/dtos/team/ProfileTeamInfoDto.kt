package com.example.amfootball.data.dtos.team

import android.net.Uri
import java.time.LocalDate

data class ProfileTeamInfoDto(
    val id: String = "",
    val name: String = "",
    val description: String = "",
    val foundationDate: LocalDate,
    val totalPoints: Int,
    val rank: String,
    val pitch: String,
    val logo: Uri = Uri.EMPTY,
    //val pitch: PitchInfo = PitchInfo(),
)
{
    companion object {
        /**
         * Gera uma instância de ProfileTeamInfoDto com dados de exemplo.
         */
        fun profileExempleTeam(): ProfileTeamInfoDto {
            return ProfileTeamInfoDto(
                id = "team_demo_001",
                name = "Os Invencíveis FC",
                description = "Equipa de exemplo para testes e demonstração. Fundada em 2024.",
                foundationDate = LocalDate.of(2024, 1, 1),
                totalPoints = 42,
                rank = "Ouro",
                pitch = "Estádio Demo (Rua Teste)",
                logo = Uri.EMPTY,
            )
        }
    }
}