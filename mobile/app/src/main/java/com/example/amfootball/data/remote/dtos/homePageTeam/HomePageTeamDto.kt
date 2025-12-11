package com.example.amfootball.data.remote.dtos.homePageTeam

import com.example.amfootball.data.remote.dtos.match.InfoMatch
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.google.gson.annotations.SerializedName

data class HomePageTeamDto(
    val team: TeamDto = TeamDto(),
    @SerializedName("NextsMatchs", alternate = ["nextsMatchs"])
    val nextsMatch: List<InfoMatch> = emptyList(),
    @SerializedName("HistoricPreviousGames", alternate = ["historicPreviousGames"])
    val vitorySequenceTeam: List<VitorySequenceTemDto> = emptyList()
)
