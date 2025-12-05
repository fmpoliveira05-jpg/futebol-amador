package com.example.amfootball.data.filters

import com.example.amfootball.data.enums.Position

/**
 * Modelo de dados que representa os filtros aplicáveis na pesquisa e listagem de jogadores.
 *
 * Todos os campos são opcionais (nullable). Se um campo for nulo, esse filtro
 * será ignorado na construção da query para a API.
 *
 * @property name O nome (ou parte do nome) do jogador a pesquisar.
 * @property city A cidade ou localidade para filtrar jogadores por localização.
 * @property minAge A idade mínima do jogador.
 * @property maxAge A idade máxima do jogador.
 * @property position A posição em campo ([Position]) do jogador (ex: Guarda-Redes, Avançado).
 * @property minSize A altura mínima do jogador em cm (Mapeado para 'MinHeight' na API).
 * @property maxSize A altura máxima do jogador em cm (Mapeado para 'MaxHeight' na API).
 */
data class FilterListPlayer(
    val name: String? = null,
    val city: String? = null,
    val minAge: Int? = null,
    val maxAge: Int? = null,
    val position: Position? = null,
    val minSize: Int? = null,
    val maxSize: Int? = null,
)

/**
 * Função de extensão que converte o objeto [FilterListPlayer] num Mapa de Strings.
 *
 * Esta conversão é necessária para utilizar a anotação `@QueryMap` do Retrofit nos pedidos GET.
 * Realiza o mapeamento entre os nomes das variáveis da App (Frontend) e os parâmetros
 * esperados pela API (Backend em C#).
 *
 * Mapeamentos importantes:
 * - `name` -> "PlayerName"
 * - `minSize/maxSize` -> "MinHeight/MaxHeight"
 * - `position` -> Envia o índice numérico (ordinal) do Enum e não o nome.
 *
 * @return Um [Map] onde a chave é o nome do parâmetro da Query string e o valor é o dado do filtro.
 */
fun FilterListPlayer.toQueryMap(): Map<String, String> {
    val map = mutableMapOf<String, String>()
    name?.let { map["PlayerName"] = it }
    city?.let { map["City"] = it }
    minAge?.let { map["MinAge"] = it.toString() }
    maxAge?.let { map["MaxAge"] = it.toString() }
    minSize?.let { map["MinHeight"] = it.toString() }
    maxSize?.let { map["MaxHeight"] = it.toString() }
    position?.let { map["Position"] = it.ordinal.toString() }

    return map
}