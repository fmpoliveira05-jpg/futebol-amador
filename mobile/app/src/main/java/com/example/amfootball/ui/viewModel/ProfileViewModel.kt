package com.example.amfootball.ui.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.App
import com.example.amfootball.data.dtos.CreateProfileDto
import com.example.amfootball.data.network.RetrofitInstance
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.launch

class ProfileViewModel : ViewModel() {

    private val sessionManager = App.sessionManager
    private val apiService = RetrofitInstance.api

    // 1. O 'Flow' que o ecrã (Activity/Fragment) vai observar
    private val _profileState = MutableStateFlow<CreateProfileDto?>(null)
    val profileState: StateFlow<CreateProfileDto?> = _profileState

    init {
        val cachedProfile = sessionManager.getUserProfile()
        if (cachedProfile != null) {
            _profileState.value = cachedProfile
        }

        // 3. Em fundo, ir buscar o perfil fresco
        fetchLatestProfile()
    }

    fun fetchLatestProfile() {
        viewModelScope.launch {
            try {
                // O Interceptor vai adicionar o token automaticamente
                val response = apiService.getMyProfile() // (Assumindo que está no ApiService)

                if (response.isSuccessful) {
                    val freshProfile = response.body()
                    if (freshProfile != null) {
                        // 4. Atualizar o 'Flow' (o ecrã atualiza-se)
                        _profileState.value = freshProfile

                        // 5. Guardar o perfil fresco no cache
                        sessionManager.saveUserProfile(freshProfile)
                    }
                }
            } catch (e: Exception) {
                // Falha de rede, não há problema, o utilizador
                // continua a ver os dados do cache.
                println("Erro ao buscar perfil: ${e.message}")
            }
        }
    }
}