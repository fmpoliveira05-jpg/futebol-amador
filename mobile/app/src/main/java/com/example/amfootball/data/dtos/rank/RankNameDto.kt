package com.example.amfootball.data.dtos.rank

/**
 * Data Transfer Object (DTO) que representa a informação básica de um Rank ou Nível.
 *
 * Utilizado tipicamente em listas de seleção (dropdowns) para filtros ou configuração.
 *
 * @property id O identificador único do Rank.
 * @property name O nome descritivo do Rank (ex: "Ouro", "Platina").
 */
data class RankNameDto(
    val id: String,
    val name: String,
) {
    companion object {
        /**
         * Gera uma lista de exemplo de Ranks para fins de teste e visualização (Preview).
         *
         * Inclui os níveis básicos de classificação: Unranked, Bronze, Silver e Platiumn.
         *
         * @return Uma lista de [RankNameDto] preenchida com dados mock.
         */
        fun generateExampleRanks(): List<RankNameDto> {
            val list = ArrayList<RankNameDto>()
            list.add(RankNameDto("0", "Todos"))
            list.add(RankNameDto("1", "Unranked"))
            list.add(RankNameDto("2", "Bronze"))
            list.add(RankNameDto("3", "Silver"))
            list.add(RankNameDto("4", "Platiumn"))

            return list
        }
    }
}
