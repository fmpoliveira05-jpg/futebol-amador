package com.example.amfootball.data.dtos.player

import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.UserRole
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa o perfil completo de um jogador.
 *
 * Este objeto agrega os dados de resposta do login (tokens e IDs do Firebase) com os
 * dados pessoais e desportivos do utilizador.
 *
 * **Características de Robustez:**
 * - **Desserialização Flexível:** Utiliza `alternate` nas anotações [SerializedName] para aceitar chaves em PascalCase ("Name") ou camelCase ("name").
 * - **Polimorfismo de Equipa:** Gere automaticamente a diferença entre o login (onde recebemos apenas o [idTeam]) e a consulta de perfil (onde recebemos o objeto [team] completo).
 * - **Conversão de Posição:** Aceita a posição como Número ou Texto da API e converte-a automaticamente para o Enum [Position].
 * - **Cálculo de Role:** Determina automaticamente o papel do utilizador ([UserRole]) com base no estado da equipa e login.
 *
 * @property loginResponseDto Objeto aninhado que contém os tokens de autenticação e identificadores do Firebase.
 * @property name O nome completo do jogador.
 * @property dateOfBirth A data de nascimento do jogador (pode ser nula).
 * @property icon A URL (String) da imagem de perfil do jogador. É `null` se não tiver foto.
 * @property address O endereço de residência do jogador.
 * @property positionRaw O valor bruto da posição recebido da API.
 * @property height A altura do jogador em centímetros.
 * @property team O objeto com detalhes da equipa (geralmente `null` durante o login).
 * @property idTeam O identificador único da equipa recebido na raiz do JSON (comum no login).
 * @property isAdmin Indica se o jogador possui privilégios de administrador na sua equipa atual.
 */
data class PlayerProfileDto(
    @SerializedName("FirebaseLoginResponseDto", alternate = ["firebaseLoginResponseDto"])
    val loginResponseDto: FireBaseLoginResponseDto? = null,
    @SerializedName("Name", alternate = ["name"])
    val name: String,
    @SerializedName("Email", alternate = ["email"])
    val email: String?,
    @SerializedName("PhoneNumber", alternate = ["phoneNumber"])
    val phoneNumber: String?,
    @SerializedName("DateOfBirth", alternate = ["dateOfBirth"])
    val dateOfBirth: String?,
    @SerializedName("Icon", alternate = ["icon", "image", "Image"])
    val icon: String? = null,
    @SerializedName("Address", alternate = ["address"])
    val address: String,
    @SerializedName("Position", alternate = ["position"])
    val positionRaw: Int,
    @SerializedName("Height", alternate = ["height"])
    val height: Int,
    @SerializedName("Team", alternate = ["team"])
    val team: TeamDto?,
    @SerializedName("IdTeam", alternate = ["idTeam"])
    val idTeam: String? = null,
    @SerializedName("IsAdmin", alternate = ["isAdmin"])
    val isAdmin: Boolean
) {
    /**
     * Propriedade auxiliar que unifica a obtenção do ID da equipa.
     *
     * Verifica primeiro se temos o objeto de equipa completo. Se não, tenta usar o ID solto.
     * Isto abstrai a inconsistência da API para quem usa este DTO.
     *
     * @return O ID da equipa ou string vazia/null se não tiver equipa.
     */
    val effectiveTeamId: String
        get() = team?.id?.takeIf { it.isNotEmpty() } ?: idTeam ?: ""

    /**
     * Propriedade auxiliar que converte o valor bruto da API para o Enum [Position].
     *
     * Transforma o [positionRaw] numa String e tenta encontrar o correspondente no Enum.
     * Se não encontrar ou for inválido, assume [Position.MIDFIELDER] por defeito.
     */
    val position: Position
        get() {
            val rawString = positionRaw.toString()
            return when (rawString) {
                "0", "Goalkeeper", "GoalKeeper" -> Position.GOALKEEPER
                "1", "Defender" -> Position.DEFENDER
                "2", "Midfielder" -> Position.MIDFIELDER
                "3", "Forward" -> Position.FORWARD
                else -> Position.MIDFIELDER
            }
        }

    /**
     * Propriedade auxiliar para verificar se o utilizador tem equipa.
     */
    val hasTeam: Boolean
        get() = effectiveTeamId.isNotEmpty()

    /**
     * Determina o papel atual do utilizador na aplicação ([UserRole]).
     *
     * A lógica segue esta ordem de prioridade:
     * 1. **UNAUTHORIZED**: Se não houver dados de login.
     * 2. **PLAYER_WITHOUT_TEAM**: Se o utilizador não tiver ID de equipa associado.
     * 3. **ADMIN_TEAM**: Se tiver equipa E a flag [isAdmin] for verdadeira.
     * 4. **MEMBER_TEAM**: Se tiver equipa mas não for administrador.
     */
    val role: UserRole
        get() {
            if (loginResponseDto == null) {
                return UserRole.UNAUTHORIZED
            }

            if (!hasTeam) {
                return UserRole.PLAYER_WITHOUT_TEAM
            }

            if (isAdmin) {
                return UserRole.ADMIN_TEAM
            }

            return UserRole.MEMBER_TEAM
        }
}