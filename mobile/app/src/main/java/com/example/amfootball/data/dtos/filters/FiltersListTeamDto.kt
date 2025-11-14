package com.example.amfootball.data.dtos.filters

data class FiltersListTeamDto(
    val name: String? = null, 
    val rank: String? = null,
    val city: String? = null,
    val minPoint: Int? = null, 
    val maxPoint: Int? = null, 
    val minAge: Int? = null,   
    val maxAge: Int? = null,   
    val minNumberMembers: Int? = null,
    val maxNumberMembers: Int? = null
)