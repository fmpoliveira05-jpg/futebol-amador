package com.example.amfootball.data.remote.dtos.player

import com.example.amfootball.domains.enums.Position
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa as informações essenciais de um jogador.
 *
 * Utilizado para listas e visualizações de perfil onde um resumo dos dados do jogador
 * (idade, posição, e status da equipa) é necessário.
 *
 * @property id O identificador único do jogador.
 * @property name O nome completo do jogador.
 * @property image A URL ou caminho da imagem de perfil do jogador (pode ser null).
 * @property address O endereço ou cidade de residência do jogador.
 * @property age A idade do jogador.
 * @property position A posição principal do jogador em campo (Enum [Position]).
 * @property heigth A altura do jogador em centímetros.
 * @property haveTeam Flag que indica se o jogador está atualmente associado a uma equipa.
 */
data class InfoPlayerDto(
    @SerializedName("Id", alternate = ["PlayerId", "playerId"])
    val id: String = "",
    val name: String = "",
    val image: String? = null,
    val address: String = "",
    val age: Int = 0,
    val position: Position,
    @SerializedName("heigth", alternate = ["height", "Height"])
    val heigth: Int = 0,
    val haveTeam: Boolean = false,
)
