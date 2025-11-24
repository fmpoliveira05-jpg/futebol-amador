package com.example.amfootball.data.local

import android.content.Context
import android.content.SharedPreferences
import com.example.amfootball.data.dtos.CreateProfileDto
import com.google.gson.Gson // Importe o Gson
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject

/**
 * Gere a sessão do utilizador, guardando e lendo o token de autenticação.
 *
 * @param context O contexto da aplicação, usado para obter SharedPreferences.
 */
class SessionManager @Inject constructor(
    @ApplicationContext context: Context
) {

    private val prefs: SharedPreferences
    private val gson = Gson()

    // Define um nome para o ficheiro de preferências e a chave para o token
    companion object {
        private const val PREFS_FILENAME = "com.example.amfootball.auth_prefs"
        private const val KEY_AUTH_TOKEN = "auth_token"
        private const val KEY_USER_PROFILE = "user_profile_json"
    }

    init {
        // Inicializa o SharedPreferences
        prefs = context.getSharedPreferences(PREFS_FILENAME, Context.MODE_PRIVATE)
    }

    /**
     * Guarda o token de autenticação no SharedPreferences.
     */
    fun saveAuthToken(token: String) {
        prefs.edit().putString(KEY_AUTH_TOKEN, token).apply()
    }

    /**
     * Obtém o token de autenticação guardado.
     *
     * @return O token como String, ou 'null' se não existir.
     */
    fun getAuthToken(): String? {
        return prefs.getString(KEY_AUTH_TOKEN, null)
    }

    /**
     * Guarda o objeto UserProfile como uma string JSON.
     */
    fun saveUserProfile(profile: CreateProfileDto) {
        val jsonString = gson.toJson(profile) // Converte objeto para JSON
        prefs.edit().putString(KEY_USER_PROFILE, jsonString).apply()
    }

    /**
     * Obtém o UserProfile a partir da string JSON guardada.
     *
     * @return O objeto UserProfileDto, ou 'null' se não existir.
     */
    fun getUserProfile(): CreateProfileDto? {
        val jsonString = prefs.getString(KEY_USER_PROFILE, null)
        return if (jsonString != null) {
            gson.fromJson(jsonString, CreateProfileDto::class.java) // Converte JSON para objeto
        } else {
            null
        }
    }

    fun clearSession() {
        prefs.edit()
            .remove(KEY_AUTH_TOKEN)
            .remove(KEY_USER_PROFILE)
            .apply()
    }
}