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
 * @property pitch O objeto [PitchInfo] que contém os dados do campo de jogo.
 */
data class ProfileTeamDto(
    val id: String = "",
    val name: String = "",
    val description: String = "",
    @SerializedName("FoundationDate", alternate = ["foundationDate"])
    val foundationDate: String? = null,
    @SerializedName("TotalPoints", alternate = ["totalPoints"])
    val totalPoints: Int = 0,
    @SerializedName("RankName", alternate = ["rankName"])
    val rank: String = "",
    val logo: String? = null,
    @SerializedName("PitchDto", alternate = ["pitchDto"])
    val pitch: PitchInfo = PitchInfo(),
)

fun ProfileTeamDto.toFormTeamDto(): FormTeamDto {
    return FormTeamDto(
        id = this.id,
        name = this.name,
        pitch = this.pitch,
        description = this.description,
    )
}