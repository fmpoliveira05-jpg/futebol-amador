package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.UiState
import com.example.amfootball.data.dtos.team.ProfileTeamDto
import com.example.amfootball.data.repository.TeamRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import javax.inject.Inject

/**
 * ViewModel da página de perfil da equipa
 * */
@HiltViewModel
class ProfileTeamViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val repository: TeamRepository
): ViewModel() {
    private val infoTeam: MutableLiveData<ProfileTeamDto> = MutableLiveData(null)
    val uiInfoTeam: LiveData<ProfileTeamDto> = infoTeam

    val teamId: String? = savedStateHandle["teamId"]

    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState

    init{
        if(teamId != null) {

        }
        //infoTeam.value = ProfileTeamInfoDto.profileExempleTeam()
    }

    /**
     * Metodo que carrega os dados de perfil da equipa, da API e guarda-los na infoTeam
     * */
    private fun loadTeamProfile(teamId: String) {

    }
}