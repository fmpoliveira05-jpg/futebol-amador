package com.example.amfootball.data.filters

import com.example.amfootball.data.enums.TypeMatch
import com.google.gson.annotations.SerializedName
import java.time.LocalDate

/**
 * Data class que contém todos os critérios de filtro aplicáveis a uma lista de partidas (calendário).
 *
 * Utiliza a anotação [SerializedName] com 'alternate' para garantir compatibilidade
 * caso o backend envie as chaves em Maiúsculas ou minúsculas (ex: "MinDate" ou "minDate").
 *
 * @property opponentName Filtro pelo nome da equipa adversária.
 * @property minGameDate Filtro pela data mínima da partida (Formato ISO: YYYY-MM-DD).
 * @property maxGameDate Filtro pela data máxima da partida (Formato ISO: YYYY-MM-DD).
 * @property gameLocale Filtro por localização: true (Casa), false (Fora), null (Todos).
 * @property typeMatch Filtro pelo tipo de partida: [TypeMatch.COMPETITIVE] (Ranked) ou [TypeMatch.CASUAL] (Amigável).
 * @property isFinish Filtro pelo estado: true (Finalizada), false (Agendada/Decorrer), null (Todos).
 */
data class FilterCalendar(
    @SerializedName("NameOpponent", alternate = ["nameOpponent"])
    val opponentName: String? = null,
    @SerializedName("MinDate", alternate = ["minDate"])
    val minGameDate: LocalDate? = null,
    @SerializedName("MaxDate", alternate = ["maxDate"])
    val maxGameDate: LocalDate? = null,
    @SerializedName("IsHome", alternate = ["isHome"])
    val gameLocale: Boolean? = null,
    @SerializedName("IsRanqued", alternate = ["isRanqued"])
    val typeMatch: TypeMatch? = null,
    @SerializedName("IsFinished", alternate = ["isFinished"])
    val isFinish: Boolean? = null,
)

/**
 * Converte o objeto de filtro num Mapa de Query Strings para o Retrofit.
 *
 * Realiza as conversões necessárias de tipos complexos (Enum, Date, Boolean) para String.
 */
fun FilterCalendar.toQueryMap(): Map<String, String> {
    val map = mutableMapOf<String, String>()
    opponentName?.let { map["PlayerName"] = it }
    minGameDate?.let { map["City"] = it.toString() }
    maxGameDate?.let { map["MinAge"] = it.toString() }
    gameLocale?.let { map["MaxAge"] = it.toString() }
    typeMatch?.let {
        val isRanked = (it == TypeMatch.COMPETITIVE)
        map["IsRanqued"] = isRanked.toString()
    }
    isFinish?.let { map["MaxHeight"] = it.toString() }

    return map
}