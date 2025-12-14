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
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.withContext
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço de comunicação em tempo real (SignalR) dedicado à gestão do início de partidas.
 *
 * Responsável por:
 * 1. Estabelecer e gerir a conexão com o Hub 'StartMatch' do backend.
 * 2. Autenticar a conexão usando o token JWT do [SessionManager].
 * 3. Enviar comandos para entrar/sair de uma sala de jogo (`JoinStartMatch`/`LeaveStartMatch`).
 * 4. Receber eventos do servidor (ex: partida iniciada) e expô-los através de um [SharedFlow].
 *
 * É configurado como [Singleton] para garantir uma única instância de conexão SignalR durante
 * o ciclo de vida da aplicação.
 *
 * @property sessionManager Gerenciador de sessão para obter o token de autenticação.
 */
@Singleton
class StartMatchSocketService @Inject constructor(
    private val sessionManager: SessionManager
) {
    /** Instância da conexão SignalR (HubConnection) com o backend. */
    private var hubConnection: HubConnection? = null

    /** Flow mutável interno para emitir eventos de partida recebidos do SignalR. */
    private val _matchEvents = MutableSharedFlow<String>(replay = 1)

    /**
     * Flow público de eventos de partida.
     *
     * Permite que os ViewModels ou outros coletores subscrevam a eventos de partida
     * em tempo real (ex: confirmação de início de jogo).
     */
    val matchEvents = _matchEvents.asSharedFlow()

    /**
     * Inicia a conexão SignalR com o Hub de início de partida de forma assíncrona.
     *
     * Cria e configura o [HubConnection] se ainda não existir, incluindo a injeção
     * do token JWT para autenticação.
     *
     * @return [Boolean] indicando `true` se a conexão foi estabelecida ou já estava ativa,
     * e `false` em caso de erro.
     */
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

                    registerHandlers()
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

    /**
     * Regista os callbacks de escuta para os eventos que o servidor SignalR pode enviar.
     *
     * Atualmente regista o evento [SignalRMethods.RECEIVE_MATCH].
     */
    private fun registerHandlers() {
        hubConnection?.on(SignalRMethods.RECEIVE_MATCH, { message ->
            Log.d("SIGNALR", "Mensagem recebida: $message")
            _matchEvents.tryEmit(message)
        }, String::class.java)
    }

    /**
     * Envia uma mensagem para o servidor para entrar no grupo de um jogo específico.
     *
     * Esta ação permite que o cliente comece a receber eventos em tempo real relacionados
     * a esse [idMatch].
     *
     * @param idMatch O identificador único da partida.
     * @param idTeam O identificador único da equipa do utilizador.
     */
    fun joinStartMatch(idMatch: String, idTeam: String) {
        if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
            hubConnection?.send(SignalRMethods.JOIN_MATCH, idMatch, idTeam)
        } else {
            Log.e("SIGNALR", "Não está conectado. Chame startConnection() primeiro.")
        }
    }

    /**
     * Chama o método 'LeaveStartMatch' do Backend para sair do grupo de notificação do jogo.
     *
     * Utilizado para parar de receber updates quando o ecrã é fechado ou a ação é concluída.
     */
    fun leaveStartMatch() {
        if (hubConnection?.connectionState == HubConnectionState.CONNECTED) {
            hubConnection?.send(SignalRMethods.LEAVE_MATCH)
        }
    }

    /**
     * Fecha a conexão SignalR e liberta os recursos.
     *
     * Deve ser chamada quando o serviço já não é necessário (ex: ao sair do ecrã relevante
     * ou ao terminar a sessão da aplicação).
     */
    fun stopConnection() {
        try {
            hubConnection?.stop()
        } catch (e: Exception) {
            Log.e("SIGNALR", "Erro ao parar conexão: ${e.message}")
        } finally {
            hubConnection = null
        }
    }
}