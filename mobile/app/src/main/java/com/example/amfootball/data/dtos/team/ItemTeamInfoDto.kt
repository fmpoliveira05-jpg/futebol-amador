package com.example.amfootball.data.dtos.team

import android.net.Uri

/**
 * Data Transfer Object (DTO) que representa as informações detalhadas e resumidas de uma Equipa
 * para exibição em listas de alto nível.
 *
 * Este objeto combina informações de identificação, estatísticas de ranking e detalhes demográficos.
 *
 * @property id O identificador único da equipa.
 * @property name O nome da equipa.
 * @property logoTeam A URI do logótipo da equipa. Padrão é [Uri.EMPTY].
 * @property description Uma breve descrição da equipa.
 * @property city A cidade de origem da equipa.
 * @property rank O nível ou nome do rank atual da equipa.
 * @property points A pontuação atual da equipa no ranking.
 * @property averageAge A idade média dos membros da equipa.
 * @property numberMembers O número atual de membros na equipa.
 */
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