package com.example.amfootball.data.local.dao

import androidx.room.Dao
import androidx.room.Insert
import androidx.room.OnConflictStrategy
import androidx.room.Query
import com.example.amfootball.data.local.entities.UserSessionEntity

@Dao
interface SessionDao {
    @Query("SELECT * FROM user_session WHERE id = 0")
    fun getSession(): UserSessionEntity?

    @Insert(onConflict = OnConflictStrategy.REPLACE)
    fun insertOrUpdateSession(session: UserSessionEntity)

    @Query("UPDATE user_session SET authToken = :token WHERE id = 0")
    fun updateAuthToken(token: String)

    @Query("UPDATE user_session SET fcmToken = :token WHERE id = 0")
    fun updateFcmToken(token: String)

    @Query("DELETE FROM user_session WHERE id = 0")
    fun clearSession()
}