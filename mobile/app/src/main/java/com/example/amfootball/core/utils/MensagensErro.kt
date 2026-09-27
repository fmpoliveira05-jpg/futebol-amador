package com.example.amfootball.core.utils

import java.io.IOException
import java.io.InterruptedIOException
import java.net.ConnectException
import java.net.SocketTimeoutException
import java.net.UnknownHostException
import javax.net.ssl.SSLException

/**
 * Converte uma exceção de um pedido à API numa mensagem para o utilizador, sem detalhes técnicos
 * (nomes de classes, endereços, stack traces).
 *
 * As mensagens de erro da API ([handleApiError]) já vêm em português e passam como estão.
 */
object MensagensErro {
    const val TEMPO_ESGOTADO = "O servidor demorou demasiado a responder. Tenta outra vez dentro de momentos."
    const val SEM_LIGACAO = "Não foi possível contactar o servidor. Verifica a ligação à internet."
    const val LIGACAO_SEGURA = "Não foi possível estabelecer uma ligação segura ao servidor."
    const val GENERICA = "Ocorreu um erro. Tenta outra vez."

    fun paraUtilizador(erro: Throwable): String = when (erro) {
        is SocketTimeoutException -> TEMPO_ESGOTADO
        // O "call timeout" do OkHttp chega como InterruptedIOException("timeout").
        is InterruptedIOException -> TEMPO_ESGOTADO
        is UnknownHostException, is ConnectException -> SEM_LIGACAO
        is SSLException -> LIGACAO_SEGURA
        is IOException -> SEM_LIGACAO
        is ErroApi -> erro.message ?: GENERICA
        else -> erro.message?.takeIf { it.isNotBlank() && it.length < 300 && !pareceTecnica(it) } ?: GENERICA
    }

    /** Mensagens de exceções da JVM/bibliotecas que não devem chegar ao ecrã. */
    private fun pareceTecnica(mensagem: String): Boolean =
        mensagem.contains("Exception") || mensagem.contains("java.") || mensagem.contains("kotlin.") ||
            mensagem.contains("http://") || mensagem.contains("https://")
}

/** Erro devolvido pela API (4xx/5xx), com a mensagem (em português) que a API enviou e o código HTTP. */
class ErroApi(mensagem: String, val codigo: Int) : Exception(mensagem)
