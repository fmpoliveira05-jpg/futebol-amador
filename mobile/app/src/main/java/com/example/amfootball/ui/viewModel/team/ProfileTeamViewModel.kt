package com.example.amfootball.ui.viewModel.team

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.team.ProfileTeamInfoDto

class ProfileTeamViewModel: ViewModel() {
    private val infoTeam: MutableLiveData<ProfileTeamInfoDto> = MutableLiveData(ProfileTeamInfoDto())
    val uiInfoTeam: LiveData<ProfileTeamInfoDto> = infoTeam

    init{
        //TODO: Carregar dados reais na API
        infoTeam.value = ProfileTeamInfoDto.profileExempleTeam()
    }

}