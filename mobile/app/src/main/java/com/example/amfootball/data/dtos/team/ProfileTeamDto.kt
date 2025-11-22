package com.example.amfootball.data.dtos.team

import com.example.amfootball.data.dtos.support.PitchInfo
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que contém toda a informação detalhada que deve aparecer
 * no ecrã de perfil de uma equipa.
 *
 * Combina dados básicos, estatísticas de ranking e informações de localização do campo.
 *
 * @property id O identificador único da equipa.
 * @property name O nome da equipa.
 * @property description A descrição ou biografia da equipa.
 * @property foundationDate A data de fundação da equipa (string formatada).
 * @property totalPoints A pontuação total atual da equipa.
 * @property rank O nível ou nome do rank atual da equipa.
 * @property logo A URL ou caminho do logótipo da equipa (pode ser null).
 * @property pitch O objeto [PitchInfo] que contém os dados do campo de jogo. A chave JSON esperada é "PitchDto" ou "pitchDto".
 */
data class ProfileTeamDto(
    val id: String = "",
    val name: String = "",
    val description: String = "",
    val foundationDate: String? = null,
    val totalPoints: Int,
    val rank: String,
    val logo: String? = null,
    @SerializedName("PitchDto", alternate = ["pitchDto"])
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