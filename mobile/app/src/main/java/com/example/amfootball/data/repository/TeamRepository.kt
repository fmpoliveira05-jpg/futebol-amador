package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.ApiBackend
import com.example.amfootball.utils.NetworkUtils
import retrofit2.Response
import javax.inject.Inject

class TeamRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getTeamProfile(teamId: String): Response<ProfileTeamDto> {
        return api.getTeamProfile(teamId = teamId)
    }

    suspend fun getListTeam(filter: FiltersListTeam?): List<ItemTeamInfoDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return try {
            val response = api.getListTeam(filters = filterMap)

            if (response.isSuccessful && response.body() != null) {
                return response.body()!!
            } else {
                val errorRaw = response.errorBody()?.string()
                val errorMsg = NetworkUtils.parseBackendError(errorRaw)
                    ?: "Erro desconhecido: ${response.code()}"

                throw Exception(errorMsg)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }
}