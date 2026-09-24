package com.example.amfootball.data.interfaces.provider

/**
 * Interface de contrato para a obtenção do Token de Dispositivo Firebase Cloud Messaging (FCM).
 *
 * Esta interface abstrai a forma como o token FCM é obtido, permitindo diferentes implementações
 * (ex: implementação Android real que se conecta ao Firebase, ou uma implementação mock para testes).
 *
 * O token é essencial para o envio de notificações push específicas para este dispositivo.
 */
interface FcmTokenProvider {
    /**
     * Solicita o token de registo (Device Token) do FCM de forma assíncrona.
     *
     * O processo de obtenção do token pode envolver chamadas de rede ou APIs nativas que
     * demoram algum tempo. Por isso, o resultado é devolvido através de um callback.
     *
     * @param onResult Lambda de callback que recebe o token FCM ([String]?) como resultado.
     * - Se o token for obtido com sucesso, a string do token é passada.
     * - Se ocorrer uma falha (ex: dispositivo sem Google Play Services, erro de rede),
     * o valor [null] é passado.
     */
    fun getDeviceToken(onResult: (String?) -> Unit)
}