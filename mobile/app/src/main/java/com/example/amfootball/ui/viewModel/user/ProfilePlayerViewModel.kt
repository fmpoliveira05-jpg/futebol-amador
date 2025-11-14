package com.example.amfootball.ui.viewModel.user

import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow

class ProfilePlayerViewModel: ViewModel() {
    private val profilePlayer = MutableStateFlow(PlayerProfileDto())
    val uiProfilePlayer = profilePlayer.asStateFlow()

    init {
        //TODO: Carregar os dados reais do user com base no seu id, na API
        profilePlayer.value = PlayerProfileDto.createExample()
    }
}