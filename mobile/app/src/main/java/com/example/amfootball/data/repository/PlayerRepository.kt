package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.ApiBackend
import com.example.amfootball.utils.handleApiError
import retrofit2.Response
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class PlayerRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getPlayerProfile(playerId: String): PlayerProfileDto {
        try {
            val response = api.getPlayerProfile(playerId = playerId)

            if (response.isSuccessful && response.body() != null) {
                return response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    suspend fun getListPlayer(filter: FilterListPlayer?): List<InfoPlayerDto> {
        try {
            val filterMap = filter?.toQueryMap() ?: emptyMap()
            val response = api.getPlayersList(filters = filterMap)

            if (response.isSuccessful && response.body() != null) {
                return response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    /*
    * Permite enviar um pedido de adesão a uma jogador
    * */
    suspend fun sendMemberShipRequestToPlayer(teamId: String, idPlayer: String) {
        try {
            val response = api.sendMemberShipRequestToPlayer(teamId = teamId, playerId = idPlayer)

            if (!response.isSuccessful || response.body() == null) {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

}