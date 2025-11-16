package com.example.amfootball.data.dtos.team

import android.net.Uri
import com.example.amfootball.data.dtos.support.PitchInfo

data class ProfileTeamInfoDto(
    val name: String = "",
    val description: String = "",
    val logo: Uri = Uri.EMPTY,
    val pitch: PitchInfo = PitchInfo(),
)
{
    companion object {
        /**
         * Gera uma instância de ProfileTeamInfoDto com dados de exemplo.
         */
        fun profileExempleTeam(): ProfileTeamInfoDto {
            return ProfileTeamInfoDto(
                name = "Os Invencíveis FC",
                description = "Equipa de exemplo para testes e demonstração. Fundada em 2024.",
                logo = Uri.EMPTY,
                pitch = PitchInfo(
                    namePitch = "Estádio Demo",
                    addressPitch = "Rua Teste, Guimarães"
                )
            )
        }
    }
}