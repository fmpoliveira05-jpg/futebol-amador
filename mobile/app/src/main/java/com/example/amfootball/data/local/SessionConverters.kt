package com.example.amfootball.data.local

import androidx.room.TypeConverter
import com.example.amfootball.data.remote.dtos.player.PlayerProfileDto
import com.google.gson.Gson

class SessionConverters {
    private val gson = Gson()

    @TypeConverter
    fun fromUserProfile(profile: PlayerProfileDto?): String? {
        return profile?.let { gson.toJson(it) }
    }

    @TypeConverter
    fun toUserProfile(json: String?): PlayerProfileDto? {
        return json?.let { gson.fromJson(it, PlayerProfileDto::class.java) }
    }
}