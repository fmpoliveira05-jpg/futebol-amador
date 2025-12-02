package com.example.amfootball.data

import android.content.Context
import android.content.SharedPreferences


import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import com.example.amfootball.ui.screens.settings.AppLanguage
import com.example.amfootball.ui.screens.settings.AppTheme
import javax.inject.Singleton

@Singleton
class SettingsStore @Inject constructor(
    @ApplicationContext context: Context
) {
    private val prefs: SharedPreferences = context.getSharedPreferences(PREFS_FILENAME, Context.MODE_PRIVATE)

    companion object {
        private const val PREFS_FILENAME = "com.example.amfootball.settings_prefs"
        const val THEME_KEY = "app_theme"
        const val LANG_KEY = "app_language"
        const val NOTIFICATIONS_KEY = "app_notifications"
    }

    // --- Getters ---
    fun getLanguage(): String {
        return prefs.getString(LANG_KEY, AppLanguage.ENGLISH.name) ?: AppLanguage.ENGLISH.name
    }

    fun getTheme(): String {
        return prefs.getString(THEME_KEY, AppTheme.SYSTEM_DEFAULT.name) ?: AppTheme.SYSTEM_DEFAULT.name
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
        prefs.edit().putString(LANG_KEY, language.code).apply()
    }

    fun saveNotifications(notifications: Boolean) {
        prefs.edit().putBoolean(NOTIFICATIONS_KEY, notifications).apply()
    }
}