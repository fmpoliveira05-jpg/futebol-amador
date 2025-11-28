package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.matchInivite.InfoMatchInviteDto
import com.example.amfootball.data.dtos.matchInivite.SendMatchInviteDto
import com.example.amfootball.data.network.ApiBackend
import com.example.amfootball.utils.handleApiError
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class MatchInviteRepository @Inject constructor(
    private val api: ApiBackend
) {
    suspend fun sendMatchInvite(teamId: String, matchInvite: SendMatchInviteDto): InfoMatchInviteDto {
        try {
            val response = api.sendMatchInvite(idTeam = teamId, matchInivite = matchInvite)

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

    suspend fun negociateMatchInvite(teamId: String, matchInvite: SendMatchInviteDto): InfoMatchInviteDto {
        try {
            val response = api.negotiateMatch(idTeam = teamId, matchInivite = matchInvite)

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