package com.example.amfootball.data.services

import com.example.amfootball.data.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.interfaces.CalendarApi
import com.example.amfootball.utils.handleApiError
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class CalendarService @Inject constructor(
    private val calendarApi: CalendarApi
) {
    suspend fun getCalendar(teamId: String, filter: FilterCalendar?): List<InfoMatchCalendar> {
        try {
            val filters = filter?.toQueryMap() ?: emptyMap()

            val response = calendarApi.getCalendar(idTeam = teamId, filters = filters)

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

    suspend fun getMatchTeam(teamId: String, matchId: String): MatchInviteDto {
        try {
            val response = calendarApi.getMatchTeam(idTeam = teamId, idMatch = matchId)

            if (response.isSuccessful && response.body() != null) {
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
            val response = calendarApi.postponeMatch(idTeam = teamId, postponeMatch = postponeMatch)

            if (!response.isSuccessful) {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw e
        }
    }

    suspend fun cancelMatch(teamId: String, matchId: String, description: String) {
        try {
            val response = calendarApi.cancelMatch(idTeam = teamId, idMatch = matchId, description = description)

            if (!response.isSuccessful) {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw e
        }
    }
}
