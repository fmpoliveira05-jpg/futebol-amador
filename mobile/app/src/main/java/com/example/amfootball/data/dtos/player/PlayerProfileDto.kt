package com.example.amfootball.data.dtos.player

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa o perfil completo de um jogador.
 *
 * Utilizado para exibir todos os detalhes do utilizador, incluindo dados de contacto,
 * atributos físicos e a equipa atual.
 *
 * @property name O nome completo do jogador.
 * @property dateOfBirth A data de nascimento do jogador (como string formatada).
 * @property icon A URL ou caminho da imagem de perfil do jogador (pode ser null).
 * @property address O endereço de residência do jogador.
 * @property phoneNumber O número de telefone de contacto.
 * @property email O endereço de email de contacto.
 * @property position A posição principal do jogador (String, ex: "Médio").
 * @property height A altura do jogador em centímetros.
 * @property team O nome da equipa à qual o jogador pertence (o nome da chave JSON esperado do backend é "TeamName").
 */
data class PlayerProfileDto(
    val name: String,
    val dateOfBirth: String?,
    val icon: String? = null,
    val address: String,
    val phoneNumber: String,
    val email: String,
    val position: String,
    val height: Int,
    @SerializedName("TeamName")
    val team: String
)