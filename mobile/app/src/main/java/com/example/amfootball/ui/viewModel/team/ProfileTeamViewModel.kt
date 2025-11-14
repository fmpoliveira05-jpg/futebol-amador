package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.team.ProfileTeamInfoDto
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow

class ProfileTeamViewModel: ViewModel() {
    private val infoTeam = MutableStateFlow(ProfileTeamInfoDto())
    val uiInfoTeam = infoTeam.asStateFlow()

    init{
        //TODO: Carregar dados reais na API
        infoTeam.value = ProfileTeamInfoDto.profileExempleTeam()
    }

}