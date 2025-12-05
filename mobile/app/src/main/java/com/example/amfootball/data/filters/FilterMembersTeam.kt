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

/**
 * Converte os critérios de filtro definidos neste objeto para um mapa de parâmetros de consulta (Query Map).
 *
 * Esta função de extensão prepara os dados para serem consumidos por clientes HTTP (como Retrofit),
 * ignorando campos nulos e transformando tipos complexos em Strings compatíveis com a API.
 *
 * **Lógica de Transformação:**
 * - Campos nulos são omitidos do mapa.
 * - `typeMember`: É convertido para a chave `"IsAdmin"`. Se for [TypeMember.ADMIN_TEAM] o valor será "true", caso contrário "false".
 * - `position`: Utiliza o nome do enum (ex: "GOALKEEPER").
 *
 * @return Um [Map] onde a chave é o nome do parâmetro esperado pela API e o valor é o dado filtrado em String.
 */
fun FilterMembersTeam.toQueryMap(): Map<String, String> {
    val map = mutableMapOf<String, String>()

    if (typeMember != null) {
        val isAdmin = (typeMember == TypeMember.ADMIN_TEAM)
        map["IsAdmin"] = isAdmin.toString()
    }

    name?.let { map["Name"] = it }
    minAge?.let { map["MinAge"] = it.toString() }
    maxAge?.let { map["MaxAge"] = it.toString() }
    position?.let { map["Position"] = it.name }

    return map
}