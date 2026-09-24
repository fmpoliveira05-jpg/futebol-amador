package com.example.amfootball.data.remote.dtos.team

import com.example.amfootball.data.remote.dtos.support.PitchInfo
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa o modelo de dados de um Formulário de Equipa
 * (para criação ou edição).
 *
 * Inclui os campos básicos da equipa e as informações do campo de jogo ([PitchFormDto]).
 *
 * @property id O identificador único da equipa. Será null se for uma nova equipa (criação).
 * @property name O nome da equipa.
 * @property description A descrição ou biografia da equipa (opcional).
 * @property image A URI da imagem/logo da equipa selecionada (pode ser null).
 * @property pitch O objeto [PitchFormDto] contendo o nome e o endereço do campo de jogo.
 */
data class FormTeamDto(
    val id: String? = null,
    @SerializedName("name", alternate = ["Name"])
    val name: String = "",
    @SerializedName("description", alternate = ["Description"])
    val description: String? = null,
    @SerializedName("icon", alternate = ["Icon", "image", "Image"])
    val image: String? = null,
    @SerializedName("homePitch", alternate = ["HomePitch", "pitchDto", "PitchDto"])
    val pitch: PitchInfo = PitchInfo()
)