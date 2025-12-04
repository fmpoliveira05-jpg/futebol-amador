package com.example.amfootball.data.services

import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.SendMatchInviteDto
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.network.interfaces.MatchInviteApi
import com.example.amfootball.utils.handleApiError
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class MatchInviteService @Inject constructor(
    private val matchInviteApi: MatchInviteApi
) {
    suspend fun sendMatchInvite(teamId: String, matchInvite: SendMatchInviteDto): InfoMatchInviteDto {
        try {
            val response = matchInviteApi.sendMatchInvite(idTeam = teamId, dto = matchInvite)

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

    suspend fun getInviteMatch(teamId: String, matchInviteId: String): MatchInviteDto {
        try {
            val response = matchInviteApi.getMatchInvite(idTeam = teamId, matchInviteId = matchInviteId)

            if (response.isSuccessful && response.body() != null) {
                return response.body()!!.first()
            } else {
                handleApiError(response)
            }
        } catch (e: Exception) {
            e.printStackTrace()
            throw Exception("Sem conexão: ${e.localizedMessage}")
        }
    }

    suspend fun negociateMatchInvite(teamId: String, matchInvite: SendMatchInviteDto): InfoMatchInviteDto {
        try {
            val response = matchInviteApi.negotiateMatch(idTeam = teamId, matchInvite = matchInvite)

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
}