package com.example.amfootball.data.dtos.leadboardDto

data class LeadboardDto(
    val Position: Int,
    val Team: InfoTeamLeadboard
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
                            Position = i,
                            Team = InfoTeamLeadboard(
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