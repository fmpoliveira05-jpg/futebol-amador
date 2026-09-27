package com.example.amfootball.data.remote.network

import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import okhttp3.mockwebserver.MockResponse
import okhttp3.mockwebserver.MockWebServer
import org.junit.After
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNotEquals
import org.junit.Assert.assertNotNull
import org.junit.Assert.assertNull
import org.junit.Before
import org.junit.Test

/** Idempotency-Key nos POST para a API (a API ignora pedidos repetidos com a mesma chave). */
class IdempotenciaInterceptorTest {

    private lateinit var servidor: MockWebServer
    private val cliente = OkHttpClient.Builder().addInterceptor(IdempotenciaInterceptor()).build()

    @Before
    fun arrancar() {
        servidor = MockWebServer()
        servidor.start()
    }

    @After
    fun parar() = servidor.shutdown()

    private fun pedir(metodo: String, chave: String? = null): String? {
        servidor.enqueue(MockResponse().setResponseCode(200))
        val corpo = if (metodo == "GET") null else "{}".toRequestBody()
        val pedido = Request.Builder().url(servidor.url("/api/Team")).method(metodo, corpo)
            .apply { if (chave != null) header(IdempotenciaInterceptor.CABECALHO, chave) }
            .build()
        cliente.newCall(pedido).execute().close()
        return servidor.takeRequest().getHeader(IdempotenciaInterceptor.CABECALHO)
    }

    @Test
    fun `cada POST leva uma chave nova`() {
        val a = pedir("POST")
        val b = pedir("POST")
        assertNotNull(a)
        assertNotEquals(a, b)
    }

    @Test
    fun `chave definida pelo chamador e mantida`() {
        assertEquals("minha-chave-123", pedir("POST", "minha-chave-123"))
    }

    @Test
    fun `GET e PUT nao levam chave`() {
        assertNull(pedir("GET"))
        assertNull(pedir("PUT"))
    }
}
