package com.example.amfootball.data.services

import android.util.Log
import com.example.amfootball.R
import com.example.amfootball.data.events.AppEvent
import com.example.amfootball.data.events.GlobalEventBus
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.manager.CalendarManager
import com.example.amfootball.data.remote.services.NotificationCallsService
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
 * * É anotada com [dagger.hilt.android.AndroidEntryPoint] para permitir a injeção de dependências via Hilt.
 *
 * @property notificationService O serviço injetado, usado para construir e exibir notificações visuais no sistema.
 * @property sessionManager O gestor de sessão injetado, usado para verificar o estado de autenticação e guardar o token FCM localmente.
 * @property notificationCallsService O serviço injetado, usado para comunicar o token FCM com o backend via API.
 */
@AndroidEntryPoint
class PushNotificationService : FirebaseMessagingService() {
    /**
     * O serviço injetado, responsável por construir e exibir notificações visuais no sistema
     * (e.g., na barra de status).
     */
    @Inject
    lateinit var notificationService: NotificationService

    /**
     * O gestor de sessão injetado, usado para manipular dados de utilizador, como o estado
     * de autenticação e guardar o Token FCM localmente.
     */
    @Inject
    lateinit var sessionManager: SessionManager

    /**
     * O serviço injetado, usado para comunicar o token FCM ou outras informações relacionadas
     * à notificação com o backend da aplicação via API.
     */
    @Inject
    lateinit var notificationCallsService: NotificationCallsService

    /**
     * O Event Bus global injetado, usado para emitir eventos (como a eliminação de uma equipa)
     * para outras partes da aplicação (e.g., ViewModels ou Activities) que estejam a ouvir.
     */
    @Inject
    lateinit var globalEventBus: GlobalEventBus

    /**
     * O gestor de calendário injetado, usado para sincronizar eventos com o Calendário do Android.
     */
    @Inject
    lateinit var calendarManager: CalendarManager

    /**
     * Chamado quando um novo Token de Dispositivo FCM é gerado.
     *
     * O token pode ser gerado na primeira inicialização da aplicação, durante uma restauração
     * de dados, ou quando o token existente é rotacionado (atualizado) pelo Firebase
     * por motivos de segurança.
     *
     * **Processo:**
     * 1. O novo [token] é guardado localmente via [sessionManager].
     * 2. Verifica-se se o utilizador está autenticado ([sessionManager.getAuthToken]).
     * 3. Se autenticado, o token é enviado ao backend numa Coroutine I/O para registo,
     * garantindo que as futuras notificações sejam enviadas para o endereço correto.
     *
     * @param token O novo token de dispositivo FCM.
     * @see FirebaseMessagingService.onNewToken
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

    /**
     * Chamado quando uma mensagem FCM é recebida.
     *
     * Este é o método central para o tratamento de mensagens. É invocado quando:
     * 1. A aplicação está em primeiro plano (foreground) e recebe qualquer tipo de mensagem (notification ou data).
     * 2. A aplicação está em segundo plano ou fechada, mas a mensagem contém apenas um payload de dados (data payload).
     *
     * A lógica implementada usa o valor `type` (extraído do payload de dados) para rotear
     * a mensagem para o handler específico (e.g., [handleTeamDeleted]).
     *
     * @param message A mensagem completa recebida, encapsulada num [com.google.firebase.messaging.RemoteMessage].
     * @see FirebaseMessagingService.onMessageReceived
     */
    override fun onMessageReceived(message: RemoteMessage) {
        super.onMessageReceived(message)

        val eventType = message.data["type"]

        when (eventType) {
            "TEAM_DELETED" -> {
                handleTeamDeleted(message = message)
            }
            "TEAM_PROMOTE" -> {
                handlePromoteTeam(message = message)
            }
            "TEAM_DEMOTION" -> {
                handleDemoteTeam(message = message)
            }
            "POST_PONE_MATCH" -> {
                handlePostPoneMatch(message = message)
            }
            "CANCEL_MATCH" -> {
                handleCancelMatch(message = message)
            }
            "ACCEPT_MATCH_INVITE" -> {
                handleAcceptMatchInvite(message = message)
            }
            "ACCEPT_MEMBERSHIP_REQUEST_PLAYER_NOTIFY" -> {
                handleAcceptMemberShipRequestByTeam(message = message)
            }
            else -> {
                handleDefaultMessageReceiver(message = message)
            }
        }
    }

    /**
     * Trata mensagens genéricas ou mensagens que não contêm um `type` reconhecido.
     *
     * Tenta extrair o título e o corpo da notificação do payload `data` ou do payload `notification`.
     * Se os campos não existirem no `data` payload, utiliza *strings* de fallback genéricas.
     *
     * @param message A [RemoteMessage] recebida.
     */
    private fun handleDefaultMessageReceiver(message: RemoteMessage) {
        if (message.data.isNotEmpty()) {
            val title = message.data["title"] ?: "Nova Notificação"
            val body = message.data["body"] ?: "Tens uma nova mensagem"

            notificationService.showNotificationTeam(title, body)
        }

        message.notification?.let {
            notificationService.showNotificationTeam(
                title = it.title ?: "", message = it.body ?: ""
            )
        }
    }

    /**
     * Trata o evento de "Equipa Eliminada" (TEAM_DELETED).
     *
     * **Ações:**
     * 1. Usa strings localizadas (via [getString(R.string.x)] no caso do código em PT) ou o texto do payload para o título e corpo.
     * 2. Limpa o `teamId` do utilizador no [sessionManager], indicando que já não faz parte de uma equipa.
     * 3. Emite um evento [com.example.amfootball.data.events.AppEvent.TeamDeleted] no [globalEventBus] para que a UI possa reagir (e.g., navegar para a Home).
     * 4. Exibe a notificação com uma ação de navegação.
     *
     * @param message A [RemoteMessage] recebida.
     */
    private fun handleTeamDeleted(message: RemoteMessage) {
        val title = message.data["title"] ?: getString(R.string.notif_team_deleted_title)
        val message = message.data["body"] ?: getString(R.string.notif_team_deleted_body)

        sessionManager.updateTeamIdUser(null)

        CoroutineScope(Dispatchers.Main).launch {
            globalEventBus.emitEvent(AppEvent.UpdateHomePage(message))
        }

        notificationService.showNotificationTeam(
            title = title,
            message = message,
            navigationAction = "NAVIGATE_TO_HOME"
        )
    }

    /**
     * Trata o evento de "Promoção a Administrador" (TEAM_PROMOTE).
     *
     * **Ações:**
     * 1. Obtém as strings localizadas (via [getString(R.string.x)]) ou do payload.
     * 2. Atualiza a função do membro da equipa para `true` (Administrador) no [sessionManager].
     * 3. Emite um evento (atualmente [AppEvent.TeamDeleted], **Nota**: idealmente deveria ser [AppEvent.UserPromoted]) no [globalEventBus].
     * 4. Exibe a notificação.
     *
     * @param message A [RemoteMessage] recebida.
     */
    private fun handlePromoteTeam(message: RemoteMessage) {
        val title = message.data["title"] ?: getString(R.string.notif_promote_title)
        val message = message.data["body"] ?: getString(R.string.notif_promote_body)

        sessionManager.updateRoleMemberTeam(true)

        CoroutineScope(Dispatchers.Main).launch {
            globalEventBus.emitEvent(AppEvent.UpdateHomePage(message))
        }

        notificationService.showNotificationTeam(
            title = title,
            message = message,
            navigationAction = "NAVIGATE_TO_HOME"
        )
    }

    /**
     * Trata o evento de "Despromoção a Jogador" (TEAM_DEMOTION).
     *
     * **Ações:**
     * 1. Obtém as strings localizadas (via [getString(R.string.x)]) ou do payload.
     * 2. Atualiza a função do membro da equipa para `false` (Jogador regular) no [sessionManager].
     * 3. Emite um evento (atualmente [AppEvent.TeamDeleted], **Nota**: idealmente deveria ser [AppEvent.UserDemoted]) no [globalEventBus].
     * 4. Exibe a notificação.
     *
     * @param message A [RemoteMessage] recebida.
     */
    private fun handleDemoteTeam(message: RemoteMessage) {
        val title = message.data["title"] ?: getString(R.string.notif_demote_title)
        val message = message.data["body"] ?: getString(R.string.notif_demote_body)

        sessionManager.updateRoleMemberTeam(false)

        CoroutineScope(Dispatchers.Main).launch {
            globalEventBus.emitEvent(AppEvent.UpdateHomePage(message))
        }

        notificationService.showNotificationTeam(
            title = title,
            message = message,
            navigationAction = "NAVIGATE_TO_HOME"
        )
    }

    /**
     * Trata o evento de "Adiamento de Partida" (POST_PONE_MATCH)
     *
     * Este handler é executado quando o Backend notifica o utilizador (Membro da Equipa ou Adversário)
     * sobre uma mudança na data do jogo (após o aceite de um pedido de adiamento).
     *
     * **Ações:**
     * 1. Extrai o `matchId` e os novos timestamps (`newDateMillis`) do payload.
     * 2. Calcula um horário de fim padrão (assumindo 2 horas de duração, 7.200.000 ms).
     * 3. Chama [CalendarManager.updateMatchDateOnly] para atualizar silenciosamente o evento no calendário local
     * (se o utilizador o tiver adicionado).
     * 4. Exibe uma notificação visual.
     *
     * @param message A [RemoteMessage] recebida contendo o payload de dados.
     */
    private fun handlePostPoneMatch(message: RemoteMessage) {
        val matchId = message.data["matchId"]
        val newDateMillisString = message.data["newDateMillis"]
        val newDateMillis = newDateMillisString?.toLongOrNull()

        val title = message.data["title"] ?: "Jogo Reagendado"
        val body = message.data["body"] ?: "A data do jogo foi alterada."

        if (matchId != null && newDateMillis != null) {
            val endTime = newDateMillis + (2 * 60 * 60 * 1000)

            calendarManager.updateMatchDateOnly(
                matchId = matchId,
                newStart = newDateMillis,
                newEnd = endTime
            )
        } else {
            Log.e("FCM_DEBUG", "Dados inválidos recebidos para adiar jogo.")
        }

        notificationService.showNotificationTeam(
            title = title,
            message = body,
        )
    }

    /**
     * Trata o evento de "Cancelamento de Partida" (CANCEL_MATCH).
     *
     * Este handler é executado quando o Backend notifica os utilizadores que uma partida foi cancelada.
     *
     * **Ações:**
     * 1. Extrai o `matchId` do payload.
     * 2. Chama [CalendarManager.removeMatch] para remover o evento do calendário local do utilizador
     * (se o utilizador o tiver adicionado).
     * 3. Exibe uma notificação visual.
     *
     * @param message A [RemoteMessage] recebida.
     */
    private fun handleCancelMatch(message: RemoteMessage) {
        val matchId = message.data["matchId"]

        val title = message.data["title"] ?: "Jogo cancelado"
        val body = message.data["body"] ?: "Uma das suas partidas foi cancelada."

        if (matchId != null) {
            calendarManager.removeMatch(matchId = matchId)
        }

        notificationService.showNotificationTeam(
            title = title,
            message = body,
        )
    }

    /**
     * Trata o evento de "Aceite de Convite de Partida" (ACCEPT_MATCH_INVITE).
     *
     * Este handler é executado quando uma partida passa do estado "Pendente" para "Confirmada".
     *
     * **Ações:**
     * 1. Extrai todos os detalhes necessários do calendário (`title`, `location`, `startMillis`, `endMillis`).
     * 2. Chama [CalendarManager.addMatch] para criar o evento no calendário local do utilizador.
     * (Se o utilizador tinha um evento "Pendente" no calendário, deve-se remover antes).
     * 3. Exibe uma notificação visual.
     *
     * @param message A [RemoteMessage] recebida, contendo dados completos do evento.
     */
    private fun handleAcceptMatchInvite(message: RemoteMessage) {
        val matchId = message.data["matchId"]

        val calendarTitle = message.data["calendarTitle"] ?: "Jogo de Futebol"
        val calendarDescription = message.data["calendarDescription"] ?: ""
        val location = message.data["location"] ?: "Campo"
        val startMillis = message.data["startMillis"]?.toLongOrNull()
        val endMillis = message.data["endMillis"]?.toLongOrNull()
        val notifTitle = message.data["title"] ?: "Jogo Confirmado"
        val notifBody = message.data["body"] ?: "O teu jogo foi agendado."

        if (matchId != null && startMillis != null && endMillis != null) {
            calendarManager.addMatch(
                matchId = matchId,
                title = calendarTitle,
                desc = calendarDescription,
                loc = location,
                start = startMillis,
                end = endMillis
            )
        }

        notificationService.showNotificationTeam(
            title = notifTitle,
            message = notifBody,
        )
    }

    /**
     * Trata o evento de "Aceite de Pedido de Adesão" (ACCEPT_MEMBERSHIP_REQUEST_PLAYER_NOTIFY).
     *
     * Notifica o jogador de que o seu pedido de adesão a uma equipa foi aceite.
     *
     * **Ações:**
     * 1. Extrai `teamId` e `playerId` do payload.
     * 2. Atualiza o ID da equipa do utilizador e define o seu papel como jogador regular (`false`) no [sessionManager].
     * 3. Emite um evento [AppEvent.UpdateHomePage] no [globalEventBus] para forçar a atualização da UI.
     * 4. Exibe a notificação visual.
     *
     * @param message A [RemoteMessage] recebida.
     */
    private fun handleAcceptMemberShipRequestByTeam(message: RemoteMessage) {
        val playerId = message.data["playerId"]
        val teamId = message.data["teamId"]

        val title = message.data["title"] ?: "Convite de adesão aceite"
        val body = message.data["body"] ?: "Foi adicionado a uma nova equipa."

        if (!teamId.isNullOrEmpty() && !playerId.isNullOrEmpty()) {
            sessionManager.updateTeamIdUser(teamId = teamId)
            sessionManager.updateRoleMemberTeam(false)
            CoroutineScope(Dispatchers.Main).launch {
                globalEventBus.emitEvent(AppEvent.UpdateHomePage("body"))
            }
        }

        notificationService.showNotificationTeam(
            title = title,
            message = body,
        )
    }
}