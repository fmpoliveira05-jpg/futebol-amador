package com.example.amfootball.data.remote.network

import com.example.amfootball.data.interfaces.provider.FcmTokenProvider
import com.google.firebase.messaging.FirebaseMessaging
import javax.inject.Inject

/**
 * Implementação concreta da interface [FcmTokenProvider] utilizando o SDK do Firebase Cloud Messaging (FCM).
 *
 * Esta classe é responsável por interagir com o [FirebaseMessaging] para obter o token de registo
 * único do dispositivo, necessário para o envio de notificações push.
 *
 * A injeção de dependência é realizada através do Hilt, utilizando a anotação [@Inject] no construtor,
 * permitindo que esta classe seja injetada onde a abstração [FcmTokenProvider] é requerida.
 */
class FirebaseFcmTokenProvider @Inject constructor() : FcmTokenProvider {

    /**
     * Obtém o token de registo (Device Token) do FCM de forma assíncrona.
     *
     * A obtenção do token é uma operação assíncrona que retorna um [Task] e, por isso,
     * utiliza-se um `addOnCompleteListener` para processar o resultado ou a falha.
     *
     * @param onResult Lambda de callback que recebe o token FCM ([String]?) como resultado.
     * - Se a tarefa for bem-sucedida, o token é retornado na string.
     * - Em caso de falha (task não bem-sucedida ou exceção), [null] é retornado.
     */
    override fun getDeviceToken(onResult: (String?) -> Unit) {
        try {
            FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
                if (task.isSuccessful) {
                    onResult(task.result)
                } else {
                    onResult(null)
                }
            }
        } catch (e: Exception) {
            onResult(null)
        }
    }
}