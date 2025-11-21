package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.network.ApiBackend
import retrofit2.Response
import javax.inject.Inject

class PlayerRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getPlayerProfile(
        playerId: String
    ): Response<PlayerProfileDto> {
        return api.getPlayerProfile(playerId = playerId)
    }
}