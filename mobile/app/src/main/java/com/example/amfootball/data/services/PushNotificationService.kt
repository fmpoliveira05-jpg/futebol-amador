package com.example.amfootball.data.services

import android.util.Log
import com.example.amfootball.data.local.SessionManager
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage
import dagger.hilt.android.AndroidEntryPoint
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * Serviço essencial para receber e processar mensagens do Firebase Cloud Messaging (FCM).
 *
 * Esta classe é responsável por:
 * 1. Receber payloads de notificação e dados quando a aplicação está em primeiro plano (foreground).
 * 2. Gerar e exibir notificações visuais através do [NotificationService].
 * 3. Gerenciar atualizações do token de dispositivo ([onNewToken]) e enviá-lo ao backend.
 * * É anotada com [AndroidEntryPoint] para permitir a injeção de dependências via Hilt.
 *
 * @property notificationService O serviço injetado, usado para construir e exibir notificações visuais no sistema.
 * @property sessionManager O gestor de sessão injetado, usado para verificar o estado de autenticação e guardar o token FCM localmente.
 * @property notificationCallsService O serviço injetado, usado para comunicar o token FCM com o backend via API.
 */
@AndroidEntryPoint
class PushNotificationService: FirebaseMessagingService() {
    @Inject lateinit var notificationService: NotificationService

    @Inject lateinit var sessionManager: SessionManager
    @Inject lateinit var notificationCallsService: NotificationCallsService

    /**
     * Chamado quando uma mensagem FCM é recebida.
     * * Este método é invocado quando:
     * 1. A aplicação está em primeiro plano (foreground) e recebe qualquer tipo de mensagem.
     * 2. A aplicação está em segundo plano ou fechada, mas a mensagem contém apenas um payload de dados (data payload).
     *
     * Se o payload contiver dados (data) ou uma notificação (notification), este método delega a exibição
     * ao [notificationService].
     *
     * @param message A mensagem completa recebida, contendo dados e/ou notificação.
     */
    override fun onMessageReceived(message: RemoteMessage) {
        super.onMessageReceived(message)

        if (message.data.isNotEmpty()) {
            val title = message.data["title"] ?: "Nova Notificação"
            val body = message.data["body"] ?: "Tens uma nova mensagem"

            notificationService.showNotificationTeam(title, body)
        }

        message.notification?.let {
            notificationService.showNotificationTeam(it.title ?: "", it.body ?: "")
        }
    }

    /**
     * Chamado quando um novo Token de Dispositivo FCM é gerado.
     *
     * O token pode ser gerado na primeira inicialização da aplicação ou quando o token existente
     * é rotacionado (atualizado) pelo Firebase por motivos de segurança.
     *
     * O processo garante que:
     * 1. O novo token é guardado localmente ([sessionManager]).
     * 2. Se o utilizador estiver autenticado ([sessionManager.getAuthToken]), o token é enviado
     * ao backend para garantir que as notificações futuras são enviadas para o endereço correto.
     *
     * @param token O novo token de dispositivo FCM.
     */
    override fun onNewToken(token: String) {
        super.onNewToken(token)
        sessionManager.saveFcmToken(token)

        val userToken = sessionManager.getAuthToken()
        if (!userToken.isNullOrEmpty()) {
            CoroutineScope(Dispatchers.IO).launch {
                try {
                    notificationCallsService.sendDeviceToken(token)
                } catch (e: Exception) {
                    Log.e("FCM", "Falha ao enviar token em background", e)
                }
            }
        }
    }
}