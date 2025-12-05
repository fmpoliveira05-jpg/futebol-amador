package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.fcm.DeviceTokenDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.PUT

/**
 * Interface Retrofit que define os endpoints de API para a gestão de notificações e tokens de dispositivo.
 * * Esta interface é utilizada pelo [com.example.amfootball.data.services.NotificationCallsService]
 * para comunicar o estado do token FCM (Firebase Cloud Messaging) com o backend.
 *
 * @see com.example.amfootball.data.dtos.fcm.DeviceTokenDto
 */
interface NotificationApi {

    /**
     * Envia e atualiza o token de dispositivo (FCM) de um utilizador autenticado no backend.
     *
     * Este método utiliza um pedido PUT e requer que o utilizador esteja autenticado
     * (o token JWT deve ser fornecido via [AuthInterceptor]).
     *
     * @param dto O DTO contendo o [token] FCM a ser guardado ou atualizado.
     * @return Uma [Response] vazia (<Unit>) que indica o sucesso da operação (código 200/204).
     */
    @PUT("${BaseEndpoints.PLAYER_API}/device-token")
    suspend fun updateDeviceToken(
        @Body dto: DeviceTokenDto
    ): Response<Unit>
}
