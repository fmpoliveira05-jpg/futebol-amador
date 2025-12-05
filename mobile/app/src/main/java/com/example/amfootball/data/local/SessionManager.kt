package com.example.amfootball.data.local

import android.content.Context
import android.content.SharedPreferences
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.google.gson.Gson
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Gestor centralizado de persistência de dados de sessão local.
 *
 * Esta classe utiliza [SharedPreferences] para armazenar dados sensíveis e de configuração do utilizador
 * que devem persistir entre reinícios da aplicação (como tokens de autenticação e perfil em cache).
 *
 * Como é anotada com [@Singleton], existe apenas uma instância desta classe durante todo o ciclo de vida
 * da aplicação, garantindo acesso consistente ao ficheiro de preferências.
 *
 * @property context O contexto da aplicação injetado pelo Hilt via [@ApplicationContext], garantindo que não há leaks de memória de Activities.
 */
@Singleton
class SessionManager @Inject constructor(
    @ApplicationContext context: Context
) {
    /**
     * Referência para o ficheiro de preferências do Android.
     * Inicializado em modo privado ([Context.MODE_PRIVATE]), tornando os dados acessíveis apenas por esta aplicação.
     */
    private val prefs: SharedPreferences

    /**
     * Instância do Gson utilizada para serializar/deserializar objetos complexos (como [PlayerProfileDto])
     * em Strings JSON, visto que o SharedPreferences suporta apenas tipos primitivos.
     */
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
     * Persiste o token de autenticação (JWT ou similar) no armazenamento local.
     *
     * A operação é realizada de forma assíncrona (`apply()`) para não bloquear a thread principal (UI).
     *
     * @param token A string do token recebida da API após login.
     */
    fun saveAuthToken(token: String) {
        prefs.edit().putString(KEY_AUTH_TOKEN, token).apply()
    }

    /**
     * Recupera o token de autenticação armazenado.
     *
     * Utilizado por interceptores de rede (Interceptors) para anexar o token aos cabeçalhos dos pedidos HTTP.
     *
     * @return O token como [String], ou `null` se o utilizador não estiver autenticado.
     */
    fun getAuthToken(): String? {
        return prefs.getString(KEY_AUTH_TOKEN, null)
    }

    /**
     * Serializa e persiste o perfil completo do utilizador.
     *
     * Converte o objeto [PlayerProfileDto] numa String JSON antes de salvar,
     * permitindo armazenar estruturas de dados complexas no SharedPreferences.
     *
     * @param profile O DTO contendo os dados do perfil do jogador.
     */
    fun saveUserProfile(profile: PlayerProfileDto) {
        val jsonString = gson.toJson(profile) // Converte objeto para JSON
        prefs.edit().putString(KEY_USER_PROFILE, jsonString).apply()
    }

    /**
     * Recupera e deserializa o perfil do utilizador armazenado em cache.
     *
     * @return O objeto [PlayerProfileDto] reconstruído a partir do JSON, ou `null` se não houver perfil salvo.
     */
    fun getUserProfile(): PlayerProfileDto? {
        val jsonString = prefs.getString(KEY_USER_PROFILE, null)
        return if (jsonString != null) {
            gson.fromJson(jsonString, PlayerProfileDto::class.java)
        } else {
            null
        }
    }

    /**
     * Método utilitário para obter rapidamente o ID local do utilizador autenticado.
     *
     * Evita a necessidade de carregar o objeto de perfil completo quando apenas o ID é necessário
     * (ex: para verificar se o utilizador é dono de um recurso).
     *
     * @return O ID do utilizador (String) ou `null` se não for encontrado.
     */
    fun fetchUserId(): String? {
        return getUserProfile()?.loginResponseDto?.localId
    }

    /**
     * Limpa todos os dados de sessão do utilizador.
     *
     * Deve ser invocado no momento do **Logout** para garantir que tokens e dados pessoais
     * são removidos do dispositivo.
     */
    fun clearSession() {
        prefs.edit()
            .remove(KEY_AUTH_TOKEN)
            .remove(KEY_USER_PROFILE)
            .apply()
    }
}