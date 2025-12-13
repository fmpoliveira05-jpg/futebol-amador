package com.example.amfootball.data.remote.sockets

import android.util.Log
import com.example.amfootball.core.utils.NetworkConsts
import com.example.amfootball.core.utils.SignalRMethods
import com.example.amfootball.core.utils.SignalRUrls
import com.example.amfootball.data.local.SessionManager
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import io.reactivex.rxjava3.core.Single
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class StartMatchSocketService @Inject constructor(
    private val sessionManager: SessionManager
) {
    private var hubConnection: HubConnection? = null

    private val _matchEvents = MutableSharedFlow<String>(replay = 1)
    val matchEvents = _matchEvents.asSharedFlow()

    suspend fun startConnection(): Boolean {
        return withContext(Dispatchers.IO) {
            try {
                val hubUrl = NetworkConsts.BASE_URL + SignalRUrls.START_MATCH_URL

                if (hubConnection == null) {
                    hubConnection = HubConnectionBuilder.create(hubUrl)
                        .withAccessTokenProvider(Single.defer {
                            val token = sessionManager.getAuthToken() ?: ""
                            Single.just(token)
                        })
                        .withHeader("ngrok-skip-browser-warning", "true")
                        .build()

                    hubConnection?.on(SignalRMethods.RECEIVE_MATCH, { message ->
                        Log.d("SIGNALR", "Mensagem recebida: $message")
                        _matchEvents.tryEmit(message)
                    }, String::class.java)
                }
                if (hubConnection?.connectionState != HubConnectionState.CONNECTED) {
                    hubConnection?.start()?.blockingAwait()
                    Log.d("SIGNALR", "Conectado com sucesso!")
                }
                true
            } catch (e: Exception) {
                Log.e("SIGNALR", "Erro ao conectar", e)
                false
            }

        }
    }

    fun joinStartMatch(idMatch: String, idTeam: String) {
        if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
            hubConnection?.send(SignalRMethods.JOIN_MATCH, idMatch, idTeam)
        } else {
            Log.e("SIGNALR", "Não está conectado. Chame startConnection() primeiro.")
        }
    }

    /**
     * Chama o método 'LeaveStartMatch' do Backend
     */
    fun leaveStartMatch() {
        if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
            hubConnection?.send(SignalRMethods.LEAVE_MATCH)
        }
    }

    /**
     * Fecha a conexão quando sair do ecrã ou destruir a app
     */
    fun stopConnection() {
        hubConnection?.stop()
    }
}