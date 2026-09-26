package com.example.amfootball.competicao

/** Filtros do mercado; os nulos não vão no pedido. */
data class FiltrosMercado(
    val temEquipa: Boolean? = null,
    val ligaId: String? = null,
    val nacionalidade: String = "",
    val posicao: Int? = null,
    val nome: String = "",
    val soListados: Boolean = false
) {
    fun paraQuery(): Map<String, String> = buildMap {
        temEquipa?.let { put("hasTeam", it.toString()) }
        ligaId?.takeIf { it.isNotBlank() }?.let { put("leagueId", it) }
        nacionalidade.trim().takeIf { it.isNotEmpty() }?.let { put("nationality", it) }
        posicao?.let { put("position", it.toString()) }
        nome.trim().takeIf { it.isNotEmpty() }?.let { put("name", it) }
        if (soListados) put("onlyListed", "true")
    }
}
