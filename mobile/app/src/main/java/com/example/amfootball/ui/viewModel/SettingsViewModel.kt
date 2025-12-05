package com.example.amfootball.ui.viewModel

import android.content.Context
import androidx.appcompat.app.AppCompatDelegate
import androidx.core.os.LocaleListCompat
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.SettingsStore
import com.example.amfootball.data.enums.settings.AppLanguage
import com.example.amfootball.data.enums.settings.AppTheme
import com.example.amfootball.data.local.SessionManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.util.Locale
import javax.inject.Inject

/**
 * ViewModel responsável pela lógica e gestão de estado do ecrã de Definições ([SettingsScreen]).
 *
 * Coordena a leitura e escrita de preferências do utilizador ([SettingsStore]) e gere estados
 * temporários da UI (visibilidade de diálogos) e estados de sessão ([SessionManager]).
 *
 * **Funcionalidades Críticas:**
 * - **Localização:** Altera o idioma da aplicação em tempo de execução ([AppCompatDelegate.setApplicationLocales]).
 * - **Tema:** Altera o tema visual da aplicação.
 * - **Sessão:** Exibe o estado de autenticação.
 *
 * @property repository O [SessionManager] para verificar o estado de login.
 * @property settingsStore O repositório local para leitura/escrita de preferências de Tema/Idioma.
 */
@HiltViewModel
class SettingsViewModel @Inject constructor(
    private val repository: SessionManager,
    private val settingsStore: SettingsStore
) : ViewModel() {

    /**
     * Estado interno mutável para controlar a visibilidade do diálogo de confirmação de eliminação de perfil.
     */
    private val deleteProfile = MutableStateFlow(false)

    /**
     * Fluxo de estado público que a UI observa para mostrar/ocultar o [DeleteProfileDialog].
     */
    val deleteProfileState = deleteProfile.asStateFlow()

    /**
    * Fluxo de estado do idioma atual da aplicação.
    * Inicializado lendo o valor persistido.
    */
    private var _isLoading = MutableStateFlow(false)
    val isLoading = _isLoading.asStateFlow()
    /**
    * Fluxo de estado do idioma atual da aplicação.
    * Inicializado lendo o valor persistido.
    */
    private var _language = MutableStateFlow(settingsStore.getLanguage())
    val language = _language.asStateFlow()

    /**
     * Estado interno mutável que indica se o utilizador tem uma sessão ativa.
     * Inicializado verificando se existe um token válido no armazenamento local.
     */
    private var _isUserLoggedIn = MutableStateFlow(repository.getAuthToken() != null)

    /**
     * Fluxo de estado público (apenas leitura) que a UI deve observar para determinar a navegação.
     * - `true`: O utilizador está autenticado.
     */
    val isUserLoggedIn = _isUserLoggedIn.asStateFlow()

    /**
     * Fluxo de estado do tema visual atual da aplicação.
     * Inicializado lendo o valor persistido.
     */
    private var _theme = MutableStateFlow(settingsStore.getTheme())
    val theme = _theme.asStateFlow()

    /**
     * Fluxo de estado das notificações da aplicação.
     * Inicializado lendo o valor persistido.
     */
    private var _notifications = MutableStateFlow(settingsStore.getNotifications())
    val notifications = _notifications.asStateFlow()

    /**
     * Define o estado [deleteProfileState] para `true`, exibindo o diálogo de confirmação de eliminação.
     */
    fun showDeleteProfile() {
        deleteProfile.value = true
    }

    /**
     * Define o estado [deleteProfileState] para `false`, ocultando o diálogo de confirmação.
     */
    fun hideDeleteProfile() {
        deleteProfile.value = false
    }

    /**
     * Atualiza os recursos de contexto do Android para o novo idioma.
     *
     * **Nota:** Esta função utiliza o método antigo de atualização de recursos e não é a forma
     * recomendada para alterar a localidade da aplicação em tempo de execução (Ver [changeLanguage]).
     *
     * @param context O contexto atual.
     * @param language O código do idioma (ex: "en", "pt-PT").
     * @return O novo [Context] de configuração.
     */
    fun updateResources(context: Context, language: String): Context {
        val locale = Locale(language)
        Locale.setDefault(locale)

        val config = context.resources.configuration
        config.setLocale(locale)
        config.setLayoutDirection(locale)

        return context.createConfigurationContext(config)
    }

    /**
     * Altera o idioma da aplicação.
     *
     * Utiliza [AppCompatDelegate.setApplicationLocales] (o método recomendado da AndroidX)
     * para alterar o idioma em runtime. Persiste a mudança no [SettingsStore] e atualiza o StateFlow.
     *
     * @param languageEnum O Enum [AppLanguage] da nova localidade.
     */
    fun changeLanguage(languageEnum: AppLanguage) {
        this.startLoading()
        val code = languageEnum.code

        settingsStore.saveLanguage(languageEnum)

        _language.value = code

        val localeList = LocaleListCompat.forLanguageTags(code)
        AppCompatDelegate.setApplicationLocales(localeList)
        _isLoading.value = false
    }

    /**
     * Altera o tema visual da aplicação.
     *
     * Persiste a mudança no [SettingsStore] e atualiza o StateFlow [_theme].
     *
     * @param theme O Enum [AppTheme] do novo tema.
     */
    fun changeTheme(theme: AppTheme) {
        this.startLoading()
        _theme.value = theme.name

        settingsStore.saveTheme(theme)
        _isLoading.value = false
    }

    private fun startLoading(){
        _isLoading.value = true
    }

    fun stopLoading(){
        _isLoading.value = false
    }

    /**
     * Executa a lógica de eliminação permanente do perfil do utilizador.
     *
     * **TODO:** A ser implementado. Deve fazer a chamada à API/Firebase e limpar a sessão local.
     *
     * @return `true` se a eliminação for bem-sucedida, `false` caso contrário.
     */
    fun deleteProfile(): Boolean {
        return false
    }
}