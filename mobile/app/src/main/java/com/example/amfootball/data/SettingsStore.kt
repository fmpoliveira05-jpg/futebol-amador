package com.example.amfootball.data

// --- IMPORTANTE: ADICIONE ESTA DEPENDÊNCIA NO SEU build.gradle.kts (Module: app) ---
// implementation("androidx.datastore:datastore-preferences:1.0.0")
// Depois clique em "Sync Now" no topo direito do Android Studio.

import android.content.Context
import android.content.SharedPreferences


import com.google.gson.Gson // Importe o Gson
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import com.example.amfootball.ui.screens.settings.AppLanguage
import com.example.amfootball.ui.screens.settings.AppTheme
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.map

// Criação da instância do DataStore (Singleton)
// O nome "settings" será o nome do ficheiro guardado internamente

class SettingsStore @Inject constructor(
    @ApplicationContext context: Context) {

    private val prefs: SharedPreferences

    private val gson = Gson()
    companion object {
        private const val PREFS_FILENAME = "com.example.amfootball.settings_prefs"
        val THEME_KEY = "app_theme"
        val LANG_KEY = "app_language"
        val NOTIFICATIONS_KEY = "app_notifications"
    }

    init {
        prefs = context.getSharedPreferences(PREFS_FILENAME, Context.MODE_PRIVATE)
    }

    // --- Getters ---
    fun getLanguage(): String? {
        val langName = prefs.getString(LANG_KEY, AppTheme.SYSTEM_DEFAULT.name)
        try {
            AppLanguage.valueOf(langName!!)
        } catch (e: IllegalArgumentException) {
            AppLanguage.ENGLISH
        }
        return langName

    }

    fun getTheme(): String? {
        val themeName = prefs.getString(THEME_KEY, AppLanguage.PORTUGUESE.name)
        try {
            AppTheme.valueOf(themeName!!)
        } catch (e: IllegalArgumentException) {
            AppTheme.SYSTEM_DEFAULT
        }
        return themeName
    }

    fun getNotifications(): Boolean {
        return prefs.getBoolean(NOTIFICATIONS_KEY, true)
    }
    // --- End Getters ---

    // --- SALVAR DADOS ---

    fun saveTheme(theme: AppTheme) {
        prefs.edit().putString(THEME_KEY, theme.name).apply()
    }
    fun saveLanguage(language: AppLanguage) {
        prefs.edit().putString(LANG_KEY, language.name).apply()
    }
    fun saveNotifications(notifications: Boolean) {
        prefs.edit().putBoolean(NOTIFICATIONS_KEY, notifications).apply()
    }
}