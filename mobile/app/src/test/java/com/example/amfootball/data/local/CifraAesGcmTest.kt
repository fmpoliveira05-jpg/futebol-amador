package com.example.amfootball.data.local

import org.junit.Assert.assertEquals
import org.junit.Assert.assertFalse
import org.junit.Assert.assertNotEquals
import org.junit.Assert.assertNull
import org.junit.Assert.assertTrue
import org.junit.Test
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey

/** Cifra da sessão guardada no telemóvel (token e perfil): formato, integridade e chave. */
class CifraAesGcmTest {

    private fun novaChave(): SecretKey = KeyGenerator.getInstance("AES").apply { init(256) }.generateKey()

    private val chave = novaChave()
    private val cifra = CifraAesGcm { chave }
    private val token = "eyJhbGciOiJSUzI1NiJ9.eyJzdWIiOiJ1aWQifQ.assinatura"

    @Test
    fun `cifrar e decifrar devolve o texto original`() {
        assertEquals(token, cifra.decifrar(cifra.cifrar(token)))
    }

    @Test
    fun `o valor guardado tem o prefixo e nao contem o texto em claro`() {
        val guardado = cifra.cifrar(token)!!
        assertTrue(guardado.startsWith(CifraAesGcm.PREFIXO))
        assertFalse(guardado.contains("eyJ"))
        assertFalse(guardado.contains("assinatura"))
    }

    @Test
    fun `cada cifragem usa um IV diferente`() {
        assertNotEquals(cifra.cifrar(token), cifra.cifrar(token))
    }

    @Test
    fun `valor alterado nao e aceite`() {
        val guardado = cifra.cifrar(token)!!
        val corpo = guardado.removePrefix(CifraAesGcm.PREFIXO).toCharArray()
        corpo[20] = if (corpo[20] == 'A') 'B' else 'A'
        assertNull(cifra.decifrar(CifraAesGcm.PREFIXO + String(corpo)))
    }

    @Test
    fun `outra chave nao decifra`() {
        val outra = CifraAesGcm { novaChave() }
        assertNull(outra.decifrar(cifra.cifrar(token)))
    }

    @Test
    fun `valor antigo em claro continua a ser lido e null da null`() {
        assertEquals("valor-antigo", cifra.decifrar("valor-antigo"))
        assertNull(cifra.cifrar(null))
        assertNull(cifra.decifrar(null))
    }
}
