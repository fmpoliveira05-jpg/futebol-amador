package com.example.amfootball.data.remote.network

import okhttp3.Interceptor
import okhttp3.Response
import java.util.UUID
import javax.inject.Inject

/**
 * Acrescenta `Idempotency-Key` (um UUID) a cada POST para a API que ainda não a tenha.
 *
 * O OkHttp pode repetir sozinho um pedido quando a ligação cai a meio
 * (`retryOnConnectionFailure`); como a chave é posta antes, a repetição leva a mesma chave e a API
 * devolve a resposta do primeiro em vez de criar outra equipa, convite ou pedido de adesão.
 */
class IdempotenciaInterceptor @Inject constructor() : Interceptor {

    override fun intercept(chain: Interceptor.Chain): Response {
        val pedido = chain.request()
        if (pedido.method != "POST" || pedido.header(CABECALHO) != null) {
            return chain.proceed(pedido)
        }
        return chain.proceed(pedido.newBuilder().header(CABECALHO, UUID.randomUUID().toString()).build())
    }

    companion object {
        const val CABECALHO = "Idempotency-Key"
    }
}
