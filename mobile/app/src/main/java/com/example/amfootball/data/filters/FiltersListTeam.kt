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

/**
 * Converte o objeto de filtros [FiltersListTeam] num mapa de parâmetros de consulta (Query Map).
 *
 * Esta função é utilizada para preparar os dados para chamadas de API (ex: Retrofit com @QueryMap).
 * Apenas os campos que não são nulos são adicionados ao mapa, garantindo que não são enviados
 * parâmetros vazios ou desnecessários para o backend.
 *
 * Mapeamento das chaves (App -> API):
 * - name -> "NameTeam"
 * - rank -> "NameRank"
 * - city -> "City"
 * - minPoint -> "MinNumberPoints"
 * - maxPoint -> "MaxNumberPoints"
 * - minAge -> "MinAge"
 * - maxAge -> "MaxAge"
 * - minNumberMembers -> "MinNumberPlayers"
 * - maxNumberMembers -> "MaxNumberPlayers"
 *
 * @return Um [Map] onde a chave é o nome do parâmetro esperado pela API e o valor é o filtro convertido para String.
 */
fun FiltersListTeam.toQueryMap(): Map<String, String> {
    val map = mutableMapOf<String, String>()
    name?.let { map["NameTeam"] = it }
    rank?.let { map["NameRank"] = it}
    city?.let { map["City"] = it }
    minPoint?.let { map["MinNumberPoints"] = it.toString()}
    maxPoint?.let { map["MaxNumberPoints"] = it.toString() }
    minAge?.let { map["MinAge"] = it.toString() }
    maxAge?.let { map["MaxAge"] = it.toString() }
    minNumberMembers?.let { map["MinNumberPlayers"] = it.toString() }
    maxNumberMembers?.let { map["MaxNumberPlayers"] = it.toString() }

    return map
}