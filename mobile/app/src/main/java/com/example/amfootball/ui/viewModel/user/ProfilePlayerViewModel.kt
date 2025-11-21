package com.example.amfootball.ui.viewModel.user

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.repository.PlayerRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class ProfilePlayerViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val repository: PlayerRepository
) : ViewModel() {
    private val profilePlayer: MutableStateFlow<PlayerProfileDto?> = MutableStateFlow(PlayerProfileDto())
    val uiProfilePlayer: StateFlow<PlayerProfileDto?> = profilePlayer.asStateFlow()

    val playerId: String? = savedStateHandle["playerId"]

    private val isLoadingState = MutableStateFlow(false)
    val isLoading: StateFlow<Boolean> = isLoadingState.asStateFlow()

    private val errorMessageState = MutableStateFlow<String?>(null)
    val errorMessage: StateFlow<String?> = errorMessageState.asStateFlow()

    init {
        //TODO: Carregar os dados reais do user com base no seu id, na API
        if(playerId != null) {
            loadPlayerProfile(playerId)
        } else {
            errorMessageState.value = "ID de jogador não encontrado."
        }
    }

    private fun loadPlayerProfile(playerId: String) {
        //TODO: Trocar isto pela logica de carregar o user na API
        //profilePlayer.value = PlayerProfileDto.createExample()
        viewModelScope.launch {
            isLoadingState.value = true
            errorMessageState.value = null

            try {
                //Depois trocar para playerId
                val response = repository.getPlayerProfile(playerId = "iIbMFBATjAYjPWu5dC8ezoEyzw12")

                if (response.isSuccessful && response.body() != null) {
                    profilePlayer.value = response.body()
                } else {
                    errorMessageState.value = "Erro ao carregar: ${response.code()}"
                }
            } catch (e: Exception) {
                errorMessageState.value = "Sem conexão: ${e.localizedMessage}"
            } finally {
                isLoadingState.value = false
            }
        }
    }

    fun retry() {
        playerId?.let { loadPlayerProfile(it) }
    }
}