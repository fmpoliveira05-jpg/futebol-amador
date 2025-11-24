package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.dtos.team.toFormTeamDto
import com.example.amfootball.data.filters.FiltersListTeam
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.ApiBackend
import com.example.amfootball.utils.NetworkUtils
import javax.inject.Inject

class TeamRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getTeamProfile(teamId: String): ProfileTeamDto {
        return try {
            val response = api.getTeamProfile(teamId = teamId)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
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

    suspend fun getTeamToUpdate(teamId: String): FormTeamDto {
        return try {
            val response = api.getTeamProfile(teamId = teamId)

            if (response.isSuccessful && response.body() != null) {
                val fullProfile = response.body()!!

                fullProfile.toFormTeamDto()
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

    suspend fun getListTeam(filter: FiltersListTeam?): List<ItemTeamInfoDto> {
        val filterMap = filter?.toQueryMap() ?: emptyMap()

        return try {
            val response = api.getListTeam(filters = filterMap)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
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

    suspend fun createTeam(team: FormTeamDto): String {
        return try {
            val response = api.createTeam(team = team)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
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
    suspend fun updateTeam(teamId: String, team: FormTeamDto): FormTeamDto {
        return try {
            val response = api.updateTeam(teamId = teamId, team = team)

            if (response.isSuccessful && response.body() != null) {
                response.body()!!
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