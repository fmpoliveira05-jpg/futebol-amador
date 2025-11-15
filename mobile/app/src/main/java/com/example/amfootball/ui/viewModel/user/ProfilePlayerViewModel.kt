package com.example.amfootball.ui.viewModel.user

import androidx.lifecycle.LiveData
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.player.PlayerProfileDto

class ProfilePlayerViewModel: ViewModel() {
    private val profilePlayer: MutableLiveData<PlayerProfileDto> = MutableLiveData(PlayerProfileDto())
    val uiProfilePlayer: LiveData<PlayerProfileDto> = profilePlayer

    init {
        //TODO: Carregar os dados reais do user com base no seu id, na API
        profilePlayer.value = PlayerProfileDto.createExample()
    }
}