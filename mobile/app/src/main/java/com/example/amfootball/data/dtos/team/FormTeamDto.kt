package com.example.amfootball.data.dtos.team

import android.net.Uri
import com.example.amfootball.data.dtos.PitchFormDto
import com.example.amfootball.data.dtos.support.PitchInfo
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
    @SerializedName("Name")
    val name: String = "",
    @SerializedName("Description")
    val description: String? = null,
    val image: Uri? = null,
    @SerializedName("PitchDto", alternate = ["pitchDto"])
    val pitch: PitchInfo = PitchInfo()
)