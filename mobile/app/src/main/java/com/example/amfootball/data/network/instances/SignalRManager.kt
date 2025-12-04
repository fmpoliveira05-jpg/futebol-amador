package com.example.amfootball.data.network.instances

import android.util.Log
import com.microsoft.signalr.HubConnection
import com.microsoft.signalr.HubConnectionBuilder
import io.reactivex.rxjava3.core.Single

class SignalRManager (
    private val baseUrl: String,
    private val tokenProvider: () -> Single<String>
) {
    private lateinit var hubConnection: HubConnection

    fun startConnection() {
        Log.d("SIGNALR_TEST", "1. Iniciando conexão SignalR...") // <--- LOG 1

        hubConnection = HubConnectionBuilder.create("${baseUrl}Notification")
            .withAccessTokenProvider(tokenProvider())
            .build()

        hubConnection.onClosed {
            Log.e("SIGNALR_TEST", "X. Conexão caiu! Erro: ${it?.message}") // <--- LOG ERRO
        }

        try {
            hubConnection.start().blockingAwait()
            Log.d("SIGNALR_TEST", "2. Conexão estabelecida com sucesso!") // <--- LOG 2
        } catch (e: Exception) {
            Log.e("SIGNALR_TEST", "X. Falha ao conectar: ${e.message}") // <--- LOG ERRO
            e.printStackTrace()
        }
    }

    fun listenToPromotions(onPromotionReceived: (String, String) -> Unit) {
        if (::hubConnection.isInitialized) {

            Log.d("SIGNALR_TEST", "3. Listener 'ReceiveNotification' registado.") // <--- LOG 3

            hubConnection.on("ReceiveNotification", { title, message ->

                // ESTE É O LOG MAIS IMPORTANTE: Prova que o Backend mandou e o Android recebeu
                Log.d("SIGNALR_TEST", "4. MENSAGEM RECEBIDA DO BACKEND: Título=$title | Msg=$message")

                onPromotionReceived(title, message)
            }, String::class.java, String::class.java)
        }
    }

    fun stopConnection() {
        if (::hubConnection.isInitialized) {
            hubConnection.stop()
        }
    }
}