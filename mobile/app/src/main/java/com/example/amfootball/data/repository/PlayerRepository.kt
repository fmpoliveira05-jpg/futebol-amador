package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.filters.FilterListPlayer
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.ApiBackend
import com.example.amfootball.utils.NetworkUtils
import retrofit2.Response
import javax.inject.Inject

class PlayerRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getMyProfile(): Response<PlayerProfileDto> {
        return api.getMyProfile()
    }

    suspend fun getPlayerProfile(playerId: String): PlayerProfileDto {
        return try {
            val response = api.getPlayerProfile(playerId = playerId)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    suspend fun getListPlayer(filter: FilterListPlayer?): List<InfoPlayerDto> {
        return try {
            val filterMap = filter?.toQueryMap() ?: emptyMap()
            val response = api.getPlayersList(filters = filterMap)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    //todo: meter a função global para os repositorys
    /**
     * Função genérica para tratar erros da API.
     * Recebe qualquer Response, extrai a mensagem de erro e lança a exceção.
     */
    private fun <T> handleApiError(response: Response<T>): Nothing {
        val errorRaw = response.errorBody()?.string()
        val errorMsg = NetworkUtils.parseBackendError(errorRaw)
            ?: "Erro desconhecido: ${response.code()}"

        throw Exception(errorMsg)
    }
}