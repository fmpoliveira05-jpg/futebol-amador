package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.ApiBackend
import retrofit2.Response
import javax.inject.Inject

class PlayerRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getPlayerProfile(playerId: String): Response<PlayerProfileDto> {
        return api.getPlayerProfile(playerId = playerId)
    }

    suspend fun getListPlayer(filter: FilterListPlayer?): Response<List<InfoPlayerDto>> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return api.getPlayersList(filters = filterMap)
    }
}