package com.example.amfootball.data.dtos.team

import android.net.Uri

//Posso meter metodo default para já me retornar uma lista de equipas default
data class ItemTeamInfoDto(
    val id: String,
    val name: String,
    val logoTeam: Uri = Uri.EMPTY,
    val description: String,
    val city: String,
    val rank: String,
    val points: Int,
    val averageAge: Int,
    val numberMembers: Int
) {
    companion object {
        fun generateExampleTeams(): List<ItemTeamInfoDto> {
            val list = ArrayList<ItemTeamInfoDto>()
            list.add(ItemTeamInfoDto("EX1", "Vitoria SC", Uri.EMPTY, "Melhor equipa de Portugal", "Guimarães","Platium", 1000, 25, 32))
            list.add(ItemTeamInfoDto("EX2", "Moreira", Uri.EMPTY, "2º melhor equipa de Guimarães", "Guimarães", "Silver", 100, 27, 10))
            list.add(ItemTeamInfoDto("EX3", "Gil Vicente", Uri.EMPTY, "Barcelos","Barcelos", "UnRanked", 10, 24, 20))
            return list
        }
    }
}