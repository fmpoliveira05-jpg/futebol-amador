package com.example.amfootball.data.local

import androidx.room.TypeConverter
import com.example.amfootball.data.remote.dtos.player.PlayerProfileDto
import com.google.gson.Gson

/**
 * Conversores do Room para o perfil da sessão. O perfil inclui os tokens do Firebase, por isso é
 * guardado cifrado ([CifraLocal]); a coluna continua a ser texto, sem migração da base de dados.
 */
class SessionConverters {
    private val gson = Gson()

    @TypeConverter
    fun fromUserProfile(profile: PlayerProfileDto?): String? {
        return profile?.let { CifraLocal.cifrar(gson.toJson(it)) }
    }

    @TypeConverter
    fun toUserProfile(json: String?): PlayerProfileDto? {
        return CifraLocal.decifrar(json)?.let {
            try {
                gson.fromJson(it, PlayerProfileDto::class.java)
            } catch (e: Exception) {
                null
            }
        }
    }
}