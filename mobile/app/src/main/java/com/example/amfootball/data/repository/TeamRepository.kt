package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.network.ApiBackend
import retrofit2.Response
import javax.inject.Inject

class TeamRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getTeamProfile(
        teamId: String
    ): Response<ProfileTeamDto> {
        return api.getTeamProfile(teamId = teamId)
    }
}