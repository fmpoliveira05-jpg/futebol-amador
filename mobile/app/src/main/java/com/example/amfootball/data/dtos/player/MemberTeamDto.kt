package com.example.amfootball.data.dtos.player

import android.net.Uri
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa os detalhes de um membro de uma equipa.
 *
 * Esta classe atua como uma ponte entre a resposta JSON da API e a lógica de negócio da app.
 * Recebe tipos primitivos (Boolean, Int) através das anotações [SerializedName] e converte-os
 * automaticamente para Enums ([TypeMember], [Position]) para uso seguro na UI.
 *
 * @property id O identificador único do membro (Mapeado de "PlayerId").
 * @property name O nome completo do membro (Mapeado de "Name").
 * @property isAdmin O valor booleano bruto vindo da API que indica se é administrador (Mapeado de "IsAdmin").
 * @property image A URI da imagem de perfil. Default é [Uri.EMPTY].
 * @property age A idade do membro (Mapeado de "Age").
 * @property height A altura do membro em centímetros (Mapeado de "Height").
 * @property positionId O identificador numérico da posição vindo da API (Mapeado de "Position").
 */
data class MemberTeamDto(
    @SerializedName("PlayerId", alternate = ["playerId"])
    val id: String = "",
    @SerializedName("Name", alternate = ["name"])
    val name: String = "",
    @SerializedName("IsAdmin", alternate = ["isAdmin"])
    val isAdmin: Boolean = false,
    val image: String? = null,
    @SerializedName("Age", alternate = ["age"])
    val age: Int = 0,
    @SerializedName("Height", alternate = ["height"])
    val height: Int = 0,
    @SerializedName("Position")
    val positionId: Int = 0,
) {
    /**
     * Propriedade calculada que converte o [positionId] (Int) para o Enum [Position].
     *
     * Mapeamento:
     * - 0 -> [Position.FORWARD]
     * - 1 -> [Position.MIDFIELDER]
     * - 2 -> [Position.DEFENDER]
     * - Outro -> [Position.GOALKEEPER]
     */
    val position: Position
        get() = when(positionId) {
            0 -> Position.FORWARD
            1 -> Position.MIDFIELDER
            2 -> Position.DEFENDER
            else -> Position.GOALKEEPER
        }

    /**
     * Propriedade calculada que converte o [isAdmin] (Boolean) para o Enum [TypeMember].
     *
     * - true -> [TypeMember.ADMIN_TEAM]
     * - false -> [TypeMember.PLAYER]
     */
    val typeMember: TypeMember
        get() = when(isAdmin) {
            true -> TypeMember.ADMIN_TEAM
            else -> TypeMember.PLAYER
        }
}
