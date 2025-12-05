package com.example.amfootball.data.dtos.leadboard

/**
 * DTO que representa uma entrada individual na Tabela de Classificação.
 *
 * Associa uma posição específica (ranking) aos dados de uma equipa.
 *
 * @property position A posição numérica da equipa na tabela (ex: 1, 2, 3...).
 * @property team O objeto [InfoTeamLeadboard] contendo os detalhes da equipa nesta posição.
 */
data class LeadboardDto(
    val position: Int,
    val team: InfoTeamLeadboard
) {
    companion object {
        fun generateLeadboardExample(): List<LeadboardDto> {
            return buildList(100) {
                for (i in 1..100) {
                    val rankName = when {
                        i == 1 -> "Platina"
                        i <= 10 -> "Ouro"
                        i <= 30 -> "Prata"
                        i <= 60 -> "Bronze"
                        else -> "Unranked"
                    }

                    val points = (100 - i) * 10

                    add(
                        LeadboardDto(
                            position = i,
                            team = InfoTeamLeadboard(
                                id = "t-$i",
                                name = "Equipa Exemplo $i",
                                currentPoints = points,
                                nameRank = rankName
                            )
                        )
                    )
                }
            }
        }
    }
}