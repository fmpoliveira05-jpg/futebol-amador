package com.example.amfootball.data.dtos

//Posso meter metodo default para já me retornar uma lista de equipas default
data class ItemTeamInfoDto(
    val Id: String,
    val Name: String,
    val Description: String,
    val City: String,
    val Rank: String,
    val Points: Int,
    val AverageAge: Int,
    val NumberMembers: Int
    //Depois meter o Icon
) {
    companion object {
        fun generateExampleTeams(): List<ItemTeamInfoDto> {
            val list = ArrayList<ItemTeamInfoDto>()
            list.add(ItemTeamInfoDto("EX1", "Vitoria SC", "Melhor equipa de Portugal", "Guimarães","Platium", 1000, 25, 32))
            list.add(ItemTeamInfoDto("EX2", "Moreira", "2º melhor equipa de Guimarães", "Guimarães", "Silver", 100, 27, 10))
            list.add(ItemTeamInfoDto("EX3", "Gil Vicente", "Barcelos","Barcelos", "UnRanked", 10, 24, 20))
            return list
        }
    }
}