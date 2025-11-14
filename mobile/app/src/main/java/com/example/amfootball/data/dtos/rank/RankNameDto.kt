package com.example.amfootball.data.dtos.rank

data class RankNameDto(
    val Id: String,
    val Name: String,
) {
    companion object {
        fun generateExampleRanks(): List<RankNameDto> {
            val list = ArrayList<RankNameDto>()
            list.add(RankNameDto("1", "Unranked"))
            list.add(RankNameDto("2", "Bronze"))
            list.add(RankNameDto("3", "Silver"))
            list.add(RankNameDto("4", "Platiumn"))

            return list
        }
    }
}
