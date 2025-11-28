package com.example.amfootball.data.dtos.player

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa o perfil completo de um jogador.
 *
 * Este objeto agrega os dados de resposta do login (tokens e IDs do Firebase) com os
 * dados pessoais e desportivos do utilizador.
 *
 * Utiliza a anotação [SerializedName] com 'alternate' em todos os campos para garantir
 * uma deserialização robusta, prevenindo erros caso o Backend envie as chaves com
 * formatação diferente (Maiúsculas/minúsculas).
 *
 * @property loginResponseDto Objeto aninhado que contém os tokens de autenticação e identificadores do Firebase.
 * @property name O nome completo do jogador.
 * @property dateOfBirth A data de nascimento do jogador (pode ser nula).
 * @property icon A URL (String) da imagem de perfil do jogador. É `null` se não tiver foto.
 * @property address O endereço de residência do jogador.
 * @property position A posição do jogador em formato de texto (ex: "Médio").
 * @property height A altura do jogador em centímetros.
 * @property team O nome da equipa à qual o jogador pertence.
 * @property idTeam O identificador único da equipa (pode ser nulo se o jogador estiver sem equipa).
 * @property isAdmin Indica se o jogador possui privilégios de administrador na sua equipa atual.
 */
data class PlayerProfileDto(
    @SerializedName("FirebaseLoginResponseDto", alternate = ["firebaseLoginResponseDto"])
    val loginResponseDto: FireBaseLoginResponseDto,
    @SerializedName("Name", alternate = ["name"])
    val name: String,
    @SerializedName("DateOfBirth", alternate = ["dateOfBirth"])
    val dateOfBirth: String?,
    @SerializedName("Icon", alternate = ["icon", "image", "Image"]) // Adicionado
    val icon: String? = null,
    @SerializedName("Address", alternate = ["address"])
    val address: String,
    @SerializedName("Position", alternate = ["position"])
    val position: String,
    @SerializedName("Height", alternate = ["height"])
    val height: Int,
    @SerializedName("TeamName", alternate = ["teamName"])
    val team: String,
    @SerializedName("IdTeam", alternate = ["idTeam"])
    val idTeam: String?,
    @SerializedName("IsAdmin", alternate = ["isAdmin"])
    val isAdmin: Boolean
)