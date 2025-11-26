package com.example.amfootball.data.dtos.support

import android.net.Uri
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) fundamental que representa a informação base de uma Equipa.
 *
 * Utilizado sempre que apenas os detalhes essenciais (ID, Nome e Imagem) são necessários,
 * evitando a transferência de grandes objetos de Equipa.
 *
 * @property id O identificador único da equipa.
 * @property name O nome da equipa.
 * @property image A URI do logótipo ou imagem da equipa (pode ser null). Padrão é [Uri.EMPTY].
 */
data class TeamDto(
    @SerializedName("IdTeam", alternate = ["idTeam"])
    val id: String,
    @SerializedName("Name", alternate = ["name"])
    val name: String,
    val image: String? = null
)