package com.example.amfootball.ui.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.network.instances.SignalRManager
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.channels.Channel
import kotlinx.coroutines.flow.receiveAsFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

@HiltViewModel
class MainViewModel @Inject constructor(
    private val signalRManager: SignalRManager
) : ViewModel(){

    private val _notificationChannel = Channel<Pair<String, String>>()
    val notificationFlow = _notificationChannel.receiveAsFlow()

    init {
        connectToSignalR()
    }

    private fun connectToSignalR() {
        viewModelScope.launch(Dispatchers.IO) {
            signalRManager.startConnection()

            signalRManager.listenToPromotions { title, message ->
                viewModelScope.launch {
                    _notificationChannel.send(title to message)
                }
            }
        }
    }

    override fun onCleared() {
        super.onCleared()
        signalRManager.stopConnection()
    }
}