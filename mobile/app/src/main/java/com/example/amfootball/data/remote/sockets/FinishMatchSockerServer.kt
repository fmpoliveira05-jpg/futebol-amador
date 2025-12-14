package com.example.amfootball.data.remote.sockets

import android.util.Log
import com.example.amfootball.core.utils.NetworkConsts
import com.example.amfootball.core.utils.SignalRMessages
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

/**
 * Gerencia a conexão via Socket (SignalR) para o processo de finalização de uma partida.
 *
 * Esta classe atua como cliente para o `FinishMatchHub` do backend, permitindo enviar resultados,
 * editar submissões e receber a confirmação de que a partida foi finalizada oficialmente.
 *
 * @property sessionManager Gerenciador de sessão utilizado para obter o token de autenticação (Bearer).
 */
@Singleton
class FinishMatchSockerServer @Inject constructor(
    private val sessionManager: SessionManager
) {
    /**
     * Instância da conexão com o Hub SignalR.
     */
    private var hubConnection: HubConnection? = null

    /**
     * Fluxo de dados mutável para emitir eventos de finalização para a UI.
     * O `replay = 1` garante que novos subscritores recebam o último estado emitido.
     */
    private val _finishEvents = MutableSharedFlow<Boolean>(replay = 1)

    /**
     * Fluxo de dados público (somente leitura) que notifica quando a partida foi finalizada com sucesso
     * (ambos os administradores concordaram com o resultado).
     */
    val finishEvents = _finishEvents.asSharedFlow()

    /**
     * Inicia a conexão com o servidor SignalR.
     *
     * Constrói a conexão se ela não existir, configura os headers de autenticação e
     * inicia a comunicação. Também registra os "listeners" para ouvir eventos vindos do servidor.
     *
     * @return `true` se a conexão foi estabelecida com sucesso, `false` caso ocorra algum erro.
     */

    suspend fun startConnection(): Boolean {
        return withContext(Dispatchers.IO) {
            try {
                val hubUrl = NetworkConsts.BASE_URL + SignalRUrls.FINISH_MATCH_URL

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
                    Log.d("SIGNALR", "FinishHub Conectado!")
                }
                true
            } catch (e: Exception) {
                false
            }
        }
    }

    /**
     * Registra os métodos que o servidor pode invocar neste cliente.
     *
     * Nota: O nome do método "ReceiveFinishMatch" deve corresponder exatamente ao que
     * o backend envia (Clients.Group(...).SendAsync("NomeDoMetodo")).
     * Ajuste conforme a implementação real do `CleanHub` ou `Service` no backend.
     */
    private fun registerHandlers() {
        hubConnection?.on(SignalRMethods.RECEIVE_FINISH_MATCH, { success: Boolean ->
            Log.d("SIGNALR", "Mensagem recebida: Jogo finalizado? $success")
            _finishEvents.tryEmit(success)
        }, Boolean::class.java)
    }

    /**
     * Envia o resultado final da partida para o servidor (JoinFinishMatch).
     *
     * Este método é chamado quando o administrador submete o resultado pela primeira vez
     * ou entra na tela de finalização.
     *
     * @param dto O objeto [ResultMatchDto] contendo o ID da partida, ID da equipa e o número de golos.
     */
    fun joinFinishMatch(dto: ResultMatchDto) {
        if (isConnected()) {
            hubConnection?.send(SignalRMethods.JOIN_FINISH_MATCH, dto)
        }
    }

    /**
     * Envia uma correção do resultado para o servidor (EditResult).
     *
     * Deve ser usado quando o utilizador já submeteu um resultado, mas precisa corrigi-lo
     * enquanto o outro administrador ainda não confirmou/finalizou.
     *
     * @param dto O objeto [ResultMatchDto] com os novos valores corrigidos.
     */
    fun editResult(dto: ResultMatchDto) {
        if (isConnected()) {
            hubConnection?.send(SignalRMethods.EDIT_RESULT, dto)
        }
    }

    /**
     * Solicita a saída do processo de finalização (LeaveFinishMatch).
     *
     * Essencial para o fluxo de "Editar": O utilizador deve sair do Hub (chamando este método),
     * desbloquear o formulário na UI, e depois voltar a chamar [joinFinishMatch] ou [editResult].
     */
    fun leaveFinishMatch() {
        if (isConnected()) {
            hubConnection?.send(SignalRMethods.LEAVE_FINISH_MATCH)
        }
    }

    /**
     * Encerra a conexão com o Hub SignalR.
     *
     * Deve ser chamado quando o ViewModel é limpo (onCleared) ou quando o utilizador
     * sai definitivamente da tela de finalização.
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

    /**
     * Verifica se a conexão com o socket está ativa.
     *
     * @return `true` se o estado for [HubConnectionState.CONNECTED], `false` caso contrário.
     */
    private fun isConnected() = hubConnection?.connectionState == HubConnectionState.CONNECTED
}