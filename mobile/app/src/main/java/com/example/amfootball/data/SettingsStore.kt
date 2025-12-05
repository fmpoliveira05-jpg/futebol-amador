package com.example.amfootball.data

import android.content.Context
import android.content.SharedPreferences
import com.example.amfootball.data.enums.settings.AppLanguage
import com.example.amfootball.data.enums.settings.AppTheme
import dagger.hilt.android.qualifiers.ApplicationContext
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Repositório local responsável pela persistência das preferências e configurações da aplicação.
 *
 * Esta classe encapsula o acesso ao [SharedPreferences] do Android, fornecendo métodos
 * tipados para guardar e ler definições do utilizador (Tema, Idioma, Notificações).
 *
 * Anotada com [@Singleton], garantindo que existe apenas uma instância a gerir o ficheiro
 * de preferências durante todo o ciclo de vida da aplicação.
 *
 * @property context O contexto da aplicação injetado via Hilt, necessário para abrir o ficheiro de preferências.
 */
@Singleton
class SettingsStore @Inject constructor(
    @ApplicationContext context: Context
) {
    /**
     * Instância privada do [SharedPreferences].
     * O ficheiro é aberto em modo [Context.MODE_PRIVATE], tornando-o acessível apenas por esta aplicação.
     */
    private val prefs: SharedPreferences =
        context.getSharedPreferences(PREFS_FILENAME, Context.MODE_PRIVATE)

    companion object {
        /** Nome do ficheiro XML onde as preferências são guardadas internamente no dispositivo. */
        private const val PREFS_FILENAME = "com.example.amfootball.settings_prefs"

        /** Chave para o valor do Tema. */
        const val THEME_KEY = "app_theme"

        /** Chave para o valor do Idioma. */
        const val LANG_KEY = "app_language"

        /** Chave para o valor das Notificações. */
        const val NOTIFICATIONS_KEY = "app_notifications"
    }

    // --- Getters ---

    /**
     * Obtém o idioma atualmente guardado.
     *
     * @return O código ou nome do idioma guardado.
     * Se não existir nenhum valor, retorna o nome do idioma por defeito: [AppLanguage.ENGLISH.name].
     */
    fun getLanguage(): String {
        return prefs.getString(LANG_KEY, AppLanguage.ENGLISH.name) ?: AppLanguage.ENGLISH.name
    }

    /**
     * Obtém o tema visual atualmente configurado.
     *
     * @return O nome do tema guardado (ex: "DARK", "LIGHT").
     * Se não existir, retorna o padrão do sistema: [AppTheme.SYSTEM_DEFAULT.name].
     */
    fun getTheme(): String {
        return prefs.getString(THEME_KEY, AppTheme.SYSTEM_DEFAULT.name)
            ?: AppTheme.SYSTEM_DEFAULT.name
    }

    /**
     * Verifica se as notificações estão ativadas.
     *
     * @return `true` se as notificações estiverem ativas (ou se for a primeira vez, valor por defeito),
     * `false` caso contrário.
     */
    fun getNotifications(): Boolean {
        return prefs.getBoolean(NOTIFICATIONS_KEY, true)
    }
    // --- SALVAR DADOS ---

    /**
     * Guarda a preferência de tema selecionada pelo utilizador.
     *
     * Utiliza `.apply()` para persistir os dados de forma assíncrona, não bloqueando a UI Thread.
     *
     * @param theme O Enum [AppTheme] selecionado. O valor guardado é a propriedade `.name`.
     */
    fun saveTheme(theme: AppTheme) {
        prefs.edit().putString(THEME_KEY, theme.name).apply()
    }

    /**
     * Guarda a preferência de idioma selecionada.
     *
     * **Nota:** Guarda a propriedade `.code` do Enum (ex: "pt", "en"), o que difere
     * ligeiramente do getter padrão que usa o `.name`.
     *
     * @param language O Enum [AppLanguage] selecionado.
     */
    fun saveLanguage(language: AppLanguage) {
        prefs.edit().putString(LANG_KEY, language.code).apply()
    }

    /**
     * Guarda a preferência de ativação/desativação de notificações.
     *
     * @param notifications `true` para ativar, `false` para desativar.
     */
    fun saveNotifications(notifications: Boolean) {
        prefs.edit().putBoolean(NOTIFICATIONS_KEY, notifications).apply()
    }
}