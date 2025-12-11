package com.example.amfootball.data.remote.dtos.homePageTeam

import com.example.amfootball.data.remote.dtos.match.InfoMatch
import com.example.amfootball.data.remote.dtos.support.TeamDto

data class HomePageTeamDto(
    val team: TeamDto = TeamDto(),
    val nextsMatch: List<InfoMatch> = emptyList(),
    val vitorySequenceTeam: List<VitorySequenceTemDto> = emptyList()
)
