package com.example.amfootball.data.filters

import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

data class FilterMembersTeam(
    val typeMember: TypeMember? = null,
    val name: String? = null,
    val minAge: Int? = null,
    val maxAge: Int? = null,
    val position: Position? = null,
)