package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.ApiBackend
import com.example.amfootball.utils.handleApiError
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class CalendarRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun getCalendar(teamId: String, filter: FilterCalendar?): List<InfoMatchCalendar> {
        try {
            val filters = filter?.toQueryMap() ?: emptyMap()

            val response = api.getCalendar(idTeam = teamId, filters = filters)

            if (response.isSuccessful) {
                return response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw e
        }
    }

    suspend fun postPoneMatch(teamId: String, postponeMatch: PostPoneMatchDto) {
        try {
            val response = api.postponeMatch(idTeam = teamId, postponeMatch = postponeMatch)

            if (response.isSuccessful) {
                return response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw e
        }
    }

    //TODO: Implementar
    suspend fun cancelMatch(teamId: String, matchId: String, description: String) {
        try {
            val response = api.cancelMatch(idTeam = teamId, idMatch = matchId, description = description)

            if (response.isSuccessful) {
                return response.body()!!
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw e
        }
    }
}
