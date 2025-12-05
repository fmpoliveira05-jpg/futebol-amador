package com.example.amfootball.data.dtos.player

import com.google.gson.annotations.SerializedName

/**
 * Objeto de Transferência de Dados (DTO) utilizado para o registo de um novo perfil de Jogador.
 *
 * Este objeto encapsula todos os dados necessários para criar uma conta de utilizador associada
 * a um perfil desportivo na API. Utiliza anotações [SerializedName] para mapear as propriedades
 * Kotlin (camelCase) para os campos JSON (PascalCase) exigidos pelo backend.
 *
 * @property userName O nome completo ou de exibição do jogador.
 * Mapeado do campo JSON `"Name"`.
 *
 * @property dateOfBirth A data de nascimento do jogador em formato de texto.
 * Mapeado do campo JSON `"DateOfBirth"`.
 * **Nota:** O formato da string deve respeitar o padrão esperado pela API (geralmente ISO-8601 "YYYY-MM-DD").
 *
 * @property phone O número de telemóvel para contacto.
 * Mapeado do campo JSON `"Phone"`.
 *
 * @property email O endereço de correio eletrónico, usado como identificador de login.
 * Mapeado do campo JSON `"Email"`.
 *
 * @property password A palavra-passe escolhida pelo utilizador (em texto simples).
 * Mapeado do campo JSON `"Password"`.
 *
 * @property address A morada ou localidade de residência do jogador.
 * Mapeado do campo JSON `"Address"`.
 *
 * @property position O identificador numérico (ID ou Enum ordinal) que representa a posição preferida do jogador em campo.
 * Mapeado do campo JSON `"Position"`.
 *
 * @property height A altura do jogador representada como um número inteiro (geralmente em centímetros).
 * Mapeado do campo JSON `"Height"`.
 */
data class CreateProfileDto(
    @SerializedName("Name")
    val userName: String,
    @SerializedName("DateOfBirth")
    val dateOfBirth: String,
    @SerializedName("Phone")
    val phone: String,
    @SerializedName("Email")
    val email: String,
    @SerializedName("Password")
    val password: String,
    @SerializedName("Address")
    val address: String,
    @SerializedName("Position")
    val position: Int,
    @SerializedName("Height")
    val height: Int
)
