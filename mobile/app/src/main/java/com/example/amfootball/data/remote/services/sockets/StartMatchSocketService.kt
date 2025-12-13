package com.example.amfootball.data.remote.services.sockets

import com.example.amfootball.data.local.SessionManager
import com.microsoft.signalr.HubConnection
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class StartMatchSocketService @Inject constructor(
    private val sessionManager: SessionManager
) {
    private var hubConnection: HubConnection? = null

    private val _matchEvents = MutableSharedFlow<String>()
    val matchEvents = _matchEvents.asSharedFlow()


}