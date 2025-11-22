package com.example.amfootball.data.filters

/**
 * Data class que contém todos os critérios de filtro aplicáveis a uma lista de equipas.
 *
 * Utilizado para persistir e transferir o estado de filtragem entre a UI e a camada de dados/lógica.
 * Inclui filtros por nome, localização, ranking (pontos e nível), idade média e número de membros.
 *
 * @property name Filtro pelo nome da equipa.
 * @property rank Filtro pelo nível de rank (ex: Ouro, Prata).
 * @property city Filtro pela cidade.
 * @property minPoint Pontuação mínima da equipa no ranking.
 * @property maxPoint Pontuação máxima da equipa no ranking.
 * @property minAge Idade média mínima dos membros da equipa.
 * @property maxAge Idade média máxima dos membros da equipa.
 * @property minNumberMembers Número mínimo de membros da equipa.
 * @property maxNumberMembers Número máximo de membros da equipa.
 */
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