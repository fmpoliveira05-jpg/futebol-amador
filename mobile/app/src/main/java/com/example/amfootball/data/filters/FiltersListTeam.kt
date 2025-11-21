package com.example.amfootball.data.filters

data class FiltersListTeam(
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