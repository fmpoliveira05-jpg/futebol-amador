package com.example.amfootball.data.local.entities

import androidx.room.Entity
import androidx.room.PrimaryKey
import com.example.amfootball.data.remote.dtos.player.PlayerProfileDto

@Entity(tableName = "user_session")
data class UserSessionEntity(
    @PrimaryKey val id: Int = 0,
    val authToken: String? = null,
    val fcmToken: String? = null,
    val userProfile: PlayerProfileDto? = null
)