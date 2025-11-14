package com.example.amfootball.ui.viewModel.MatchInvite

import androidx.lifecycle.ViewModel
import com.example.amfootball.data.dtos.matchInivite.MatchInviteDto
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow

class ListMatchInviteViewModel: ViewModel() {

    //private val filtersState = MutableStateFlow()

    private val listState = MutableStateFlow(MatchInviteDto())
    val uiList = listState.asStateFlow()


}