package com.example.amfootball.core.utils

import org.junit.Assert.assertEquals
import org.junit.Test
import java.io.InterruptedIOException
import java.net.SocketTimeoutException
import java.net.UnknownHostException

/** Mensagens de erro de rede mostradas ao utilizador (sem detalhes técnicos). */
class MensagensErroTest {

    @Test
    fun `tempo esgotado tem mensagem propria`() {
        assertEquals(MensagensErro.TEMPO_ESGOTADO, MensagensErro.paraUtilizador(SocketTimeoutException("timeout")))
        assertEquals(MensagensErro.TEMPO_ESGOTADO, MensagensErro.paraUtilizador(InterruptedIOException("timeout")))
    }

    @Test
    fun `sem rede`() {
        assertEquals(MensagensErro.SEM_LIGACAO, MensagensErro.paraUtilizador(UnknownHostException("api.exemplo.pt")))
    }

    @Test
    fun `mensagem da API passa como esta`() {
        assertEquals("A palavra-passe atual está incorreta.", MensagensErro.paraUtilizador(ErroApi("A palavra-passe atual está incorreta.", 400)))
    }

    @Test
    fun `mensagens tecnicas nao chegam ao ecra`() {
        assertEquals(MensagensErro.GENERICA, MensagensErro.paraUtilizador(IllegalStateException("java.lang.NullPointerException at x")))
        assertEquals(MensagensErro.GENERICA, MensagensErro.paraUtilizador(RuntimeException()))
    }
}
