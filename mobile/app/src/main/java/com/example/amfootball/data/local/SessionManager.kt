package com.example.amfootball.data.local

import android.content.Context
import android.content.SharedPreferences
import com.example.amfootball.data.local.dao.SessionDao
import com.example.amfootball.data.local.entities.UserSessionEntity
import com.example.amfootball.data.remote.dtos.player.PlayerProfileDto
import com.google.gson.Gson
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class SessionManager @Inject constructor(
    private val sessionDao: SessionDao
) {

    // Helper privado para garantir que nunca trabalhamos com nulos
    private fun getCurrentSession(): UserSessionEntity {
        return sessionDao.getSession() ?: UserSessionEntity()
    }

    /**
     * ID (Firebase) do utilizador com sessão, ou uma string vazia se não houver sessão.
     * Antes usava `!!` e a app fechava quando era chamado depois do logout.
     */
    fun fetchUserId(): String {
        return getUserProfile()?.loginResponseDto?.localId ?: ""
    }

    /** Guarda o token cifrado com uma chave do Android Keystore (ver [CifraLocal]). */
    fun saveAuthToken(token: String) {
        val current = getCurrentSession()
        sessionDao.insertOrUpdateSession(current.copy(authToken = CifraLocal.cifrar(token)))
    }

    fun getAuthToken(): String? {
        return CifraLocal.decifrar(sessionDao.getSession()?.authToken)
    }

    fun saveFcmToken(token: String) {
        val current = getCurrentSession()
        sessionDao.insertOrUpdateSession(current.copy(fcmToken = token))
    }

    fun getFcmToken(): String? {
        return sessionDao.getSession()?.fcmToken
    }

    fun saveUserProfile(profile: PlayerProfileDto) {
        val current = getCurrentSession()
        sessionDao.insertOrUpdateSession(current.copy(userProfile = profile))
    }

    fun getUserProfile(): PlayerProfileDto? {
        return sessionDao.getSession()?.userProfile
    }

    fun fetchTeamId(): String {
        return getUserProfile()?.idTeam ?: ""
    }


    fun updateTeamIdUser(teamId: String?) {
        val currentSession = getCurrentSession()
        val currentProfile = currentSession.userProfile

        if (currentProfile != null) {
            val updatedProfile = currentProfile.copy(
                idTeam = teamId,
                team = if (teamId == null) null else currentProfile.team,
                isAdmin = if (teamId == null) false else currentProfile.isAdmin
            )
            sessionDao.insertOrUpdateSession(currentSession.copy(userProfile = updatedProfile))
        }
    }

    fun updateRoleMemberTeam(isAdmin: Boolean) {
        val currentSession = getCurrentSession()
        val currentProfile = currentSession.userProfile

        if (currentProfile != null) {
            val updatedProfile = currentProfile.copy(isAdmin = isAdmin)
            sessionDao.insertOrUpdateSession(currentSession.copy(userProfile = updatedProfile))
        }
    }

    fun clearSession() {
        sessionDao.clearSession()
    }
}