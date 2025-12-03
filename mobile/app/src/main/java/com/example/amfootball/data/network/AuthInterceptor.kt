package com.example.amfootball.data.network

import com.example.amfootball.data.local.SessionManager
import okhttp3.Interceptor
import okhttp3.Response
import javax.inject.Inject

/**
 * Interceptor HTTP responsável pela injeção automática de credenciais (Token JWT).
 *
 * Esta classe é injetada via Hilt e atua como middleware no cliente OkHttp.
 * Verifica a existência de um token de sessão local e anexa-o ao cabeçalho `Authorization`
 * dos pedidos, exceto se o token não existir (cenários de Login/Registo).
 *
 * @property sessionManager O gestor de sessão injetado, utilizado para recuperar o token
 * de forma segura e reativa, sem dependências estáticas de contexto.
 */
class AuthInterceptor @Inject constructor(
    private val sessionManager: SessionManager
) : Interceptor {

    /**
     * Método obrigatório da interface [Interceptor] que processa o pedido HTTP.
     *
     * @param chain A cadeia (chain) de interceptores que contém o pedido original e a conexão.
     * @return A [Response] recebida do servidor após o processamento do pedido.
     */
    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        // Tenta recuperar o token atual.
        val token = sessionManager.getAuthToken()

        // Caso de uso: Login, Registo ou utilizador não autenticado.
        // Se não há token, o pedido segue "limpo" para evitar enviar cabeçalhos inválidos.
        if (token.isNullOrEmpty()) {
            return chain.proceed(originalRequest)
        }

        // Caso de uso: Utilizador autenticado.
        // Cria um novo Builder (os pedidos OkHttp são imutáveis) e anexa o cabeçalho padrão OAuth2/JWT.
        val newRequestBuilder = originalRequest.newBuilder()
            .header("Authorization", "Bearer $token")

        // Prossegue a execução da cadeia com o pedido modificado (autenticado).
        return chain.proceed(newRequestBuilder.build())
    }
}