package com.example.amfootball.ui.viewModel

import android.content.Context
import android.view.View
import androidx.appcompat.app.AppCompatActivity
import androidx.appcompat.app.AppCompatDelegate
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.core.os.LocaleListCompat
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.SettingsStore
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.repository.AuthRepository
import com.example.amfootball.ui.screens.settings.AppLanguage
import com.example.amfootball.ui.screens.settings.AppTheme
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.util.Locale
import javax.inject.Inject

@HiltViewModel
class SettingsViewModel @Inject constructor(
    private val repository: SessionManager,
    private val settingsStore: SettingsStore
) : ViewModel()   {
    private val deleteProfile = MutableStateFlow(false)
    val deleteProfileState = deleteProfile.asStateFlow()

    private var _isLoading = MutableStateFlow(false)
    val isLoading = _isLoading.asStateFlow()

    private var _language = MutableStateFlow(settingsStore.getLanguage())
    val language = _language.asStateFlow()

    /**
     * Estado interno mutável que indica se o utilizador tem uma sessão ativa.
     * Inicializado verificando se existe um token válido no armazenamento local.
     */
    private var _isUserLoggedIn = MutableStateFlow(repository.getAuthToken() != null)

    /**
     * Fluxo de estado público (apenas leitura) que a UI deve observar para determinar a navegação.
     * - `true`: O utilizador está autenticado (mostrar Home).
     * - `false`: O utilizador não está autenticado (mostrar Login).
     */
    val isUserLoggedIn = _isUserLoggedIn.asStateFlow()


    private var _theme = MutableStateFlow(settingsStore.getTheme())
    val theme = _theme.asStateFlow()
    private var _notifications = MutableStateFlow(settingsStore.getNotifications())
    val notifications = _notifications.asStateFlow()


    fun showDeleteProfile() {
        deleteProfile.value = true
    }

    fun hideDeleteProfile() {
        deleteProfile.value = false
    }

    fun changeLanguage(languageEnum: AppLanguage) {
        this.startLoading()
        val code = languageEnum.code

        settingsStore.saveLanguage(languageEnum)

        _language.value = code

        val localeList = LocaleListCompat.forLanguageTags(code)
        AppCompatDelegate.setApplicationLocales(localeList)
        _isLoading.value = false
    }

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

    //TODO: Acabar!!!
    fun deleteProfile(): Boolean{
        return false
    }
}