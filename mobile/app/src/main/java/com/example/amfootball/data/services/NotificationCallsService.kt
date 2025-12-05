package com.example.amfootball.data.services

import com.example.amfootball.data.dtos.fcm.DeviceTokenDto
import com.example.amfootball.data.network.interfaces.NotificationApi
import com.example.amfootball.utils.safeApiCallWithNotReturn
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Serviço responsável por orquestrar todas as chamadas de API relacionadas com a gestão
 * do token de notificação de dispositivos (FCM) no backend.
 *
 * Esta classe atua como uma camada intermédia entre a lógica de negócio (ViewModels)
 * e a interface [NotificationApi] (Retrofit), garantindo que as operações de rede
 * são executadas de forma segura e assíncrona.
 *
 * @property notificationApi A interface de API do Retrofit para chamadas relacionadas
 * com notificações, injetada via Hilt.
 */
@Singleton
class NotificationCallsService @Inject constructor(
    private val notificationApi: NotificationApi
) {
    /**
     * Envia o token de dispositivo do Firebase Cloud Messaging (FCM) para o backend.
     *
     * Esta operação é essencial para associar o dispositivo atual a um utilizador autenticado,
     * permitindo que o servidor direcione notificações push específicas. A chamada é
     * envolvida numa função de segurança para tratamento silencioso de erros de rede.
     *
     * @param token O Device Token único e atual fornecido pelo Firebase Messaging.
     * Este token é o destino das notificações futuras.
     *
     * @see com.example.amfootball.data.dtos.fcm.DeviceTokenDto
     * @see com.example.amfootball.utils.safeApiCallWithNotReturn
     */
    suspend fun sendDeviceToken(token: String) {
        safeApiCallWithNotReturn {
            notificationApi.updateDeviceToken(DeviceTokenDto(token))
        }
    }
}