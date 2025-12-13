package com.example.amfootball.data.remote.sockets

import android.util.Log
import com.example.amfootball.core.utils.NetworkConsts
import com.example.amfootball.core.utils.SignalRMethods
import com.example.amfootball.core.utils.SignalRUrls
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.match.ResultMatchDto
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import io.reactivex.rxjava3.core.Single
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class FinishMatchSockerServer @Inject constructor(
    private val sessionManager: SessionManager
) {
    private var hubConnection: HubConnection? = null

    private val _finishEvents = MutableSharedFlow<Boolean>(replay = 1)
    val finishEvents = _finishEvents.asSharedFlow()

    suspend fun startConnection(): Boolean {
        return withContext(Dispatchers.IO) {
            try {
                val hubUrl = NetworkConsts.BASE_URL + SignalRUrls.FINISH_MATCH_URL

                if(hubConnection == null) {
                    hubConnection = HubConnectionBuilder.create(hubUrl)
                        .withAccessTokenProvider(Single.defer {
                            val token = sessionManager.getAuthToken() ?: ""
                            Single.just(token)
                        })
                        .withHeader("ngrok-skip-browser-warning", "true")
                        .build()
                }

                if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
                    hubConnection?.start()?.blockingAwait()
                    Log.d("SIGNALR", "FinishHub Conectado!")
                }

                true
            } catch (e: Exception) {
                false
            }
        }
    }

    fun joinFinishMatch(dto: ResultMatchDto) {
        if (isConnected()) {
            hubConnection?.send(SignalRMethods.JOIN_FINISH_MATCH, dto)
        }
    }

    // Edita o resultado (se o user se enganou)
    fun editResult(dto: ResultMatchDto) {
        if (isConnected()) {
            hubConnection?.send(SignalRMethods.EDIT_RESULT, dto)
        }
    }

    fun stopConnection() {
        hubConnection?.stop()
    }

    private fun isConnected() = hubConnection?.connectionState == HubConnectionState.CONNECTED
}