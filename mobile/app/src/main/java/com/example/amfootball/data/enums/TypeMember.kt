package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R

/**
 * Enumeração que define a função ou tipo de associação de um membro dentro de uma equipa.
 *
 * Utilizado para distinguir entre membros com funções comuns (Jogador) e membros com
 * funções de gestão (Administrador).
 *
 * @property stringId O ID do recurso de string associado ao tipo de membro para exibição (ex: "Jogador").
 */
enum class TypeMember(@StringRes val stringId: Int) {
    /**
     * Indica que o membro é um jogador regular da equipa.
     */
    PLAYER(stringId = R.string.type_member_player),

    /**
     * Indica que o membro é um administrador com permissões de gestão sobre a equipa.
     */
    ADMIN_TEAM(stringId = R.string.type_member_admin_team)
}