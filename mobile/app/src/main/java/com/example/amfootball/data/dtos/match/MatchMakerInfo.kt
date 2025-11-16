package com.example.amfootball.data.dtos.match

import com.example.amfootball.data.dtos.team.InfoTeamMatchMaker
import java.util.UUID

data class MatchMakerInfo(
    val team: List<InfoTeamMatchMaker>,
    val pitch: String = ""
) {
    companion object {
        fun createExampleWithOneTeam(): MatchMakerInfo {
            val UnicaEquipa = InfoTeamMatchMaker(
                id = UUID.randomUUID().toString(),
                name = "Equipa Solitária",
                rank = "Ouro"
            )

            return MatchMakerInfo(
                team = listOf(UnicaEquipa)
            )
        }
    }
}
