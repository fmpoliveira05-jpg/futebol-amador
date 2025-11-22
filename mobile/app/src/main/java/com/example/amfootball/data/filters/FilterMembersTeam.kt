package com.example.amfootball.data.filters

import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

/**
 * Data class que contém todos os critérios de filtro aplicáveis a uma lista de membros de equipa.
 *
 * Utilizado para persistir e transferir o estado de filtragem entre a UI e a camada de dados/lógica.
 * Todos os campos são opcionais, permitindo filtros parciais.
 *
 * @property typeMember Filtro pelo tipo de membro na equipa ([TypeMember]), ou null (Indiferente).
 * @property name Filtro pelo nome do membro.
 * @property minAge Idade mínima do membro.
 * @property maxAge Idade máxima do membro.
 * @property position Filtro pela posição de jogo do membro ([Position]), ou null (Indiferente).
 */
data class FilterMembersTeam(
    val typeMember: TypeMember? = null,
    val name: String? = null,
    val minAge: Int? = null,
    val maxAge: Int? = null,
    val position: Position? = null,
)