package com.example.amfootball.data.dtos.player

import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que encapsula a resposta de uma autenticação via Firebase.
 *
 * Este objeto contém os tokens de segurança e as informações básicas de identificação
 * retornadas pelos serviços do Firebase Auth após um login bem-sucedido.
 *
 * @property idToken O token de identidade (JWT) do Firebase. É este token que deve ser enviado para o Backend da API para autenticar os pedidos.
 * @property refreshToken O token de atualização, usado para obter um novo [idToken] quando este expirar, sem obrigar o utilizador a fazer login novamente.
 * @property expiresIn A duração em segundos até que o [idToken] expire (geralmente "3600" para 1 hora).
 * @property localId O identificador único do utilizador (UID) dentro do sistema Firebase.
 * @property phoneNumber O número de telefone associado à conta (se existir).
 * @property email O endereço de email do utilizador autenticado.
 */
data class FireBaseLoginResponseDto(
    @SerializedName("idToken", alternate = ["IdToken"])
    val idToken: String,
    @SerializedName("refreshToken", alternate = ["RefreshToken"])
    val refreshToken: String,
    @SerializedName("expiresIn", alternate = ["ExpiresIn"])
    val expiresIn: String,
    @SerializedName("localId", alternate = ["LocalId"])
    val localId: String,
    val phoneNumber: Int,
    @SerializedName("email", alternate = ["Email"])
    val email: String,
)