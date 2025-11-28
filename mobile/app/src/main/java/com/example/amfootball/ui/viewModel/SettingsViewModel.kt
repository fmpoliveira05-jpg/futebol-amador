package com.example.amfootball.ui.viewModel

import android.content.Context
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.repository.AuthRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import java.util.Locale
import javax.inject.Inject

@HiltViewModel
class SettingsViewModel @Inject constructor(
    private val repository: AuthRepository
) : ViewModel() {
    private val deleteProfile = MutableStateFlow(false)
    val deleteProfileState = deleteProfile.asStateFlow()
    /**
     * Estado interno mutável que indica se o utilizador tem uma sessão ativa.
     * Inicializado verificando se existe um token válido no armazenamento local.
     */
    private val _isUserLoggedIn = MutableStateFlow(repository.isUserLoggedIn())

    /**
     * Fluxo de estado público (apenas leitura) que a UI deve observar para determinar a navegação.
     * - `true`: O utilizador está autenticado (mostrar Home).
     * - `false`: O utilizador não está autenticado (mostrar Login).
     */
    val isUserLoggedIn = _isUserLoggedIn.asStateFlow()

    fun updateResources(context: Context, language: String): Context {
        val locale = Locale(language)
        Locale.setDefault(locale)

        val config = context.resources.configuration
        config.setLocale(locale)
        config.setLayoutDirection(locale)

        return context.createConfigurationContext(config)
    }
    //TODO: Acabar!!!
    fun deleteProfile(): Boolean{
        return false
    }
}