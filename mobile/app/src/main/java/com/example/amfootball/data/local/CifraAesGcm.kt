package com.example.amfootball.data.local

import java.util.Base64
import javax.crypto.Cipher
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

/**
 * AES-GCM com uma chave fornecida (em produção, a do Android Keystore — ver [CifraLocal]).
 *
 * Separado do Keystore para poder ser testado na JVM (testes unitários, sem aparelho).
 * Formato: `enc1:` + Base64(IV de 12 bytes + texto cifrado com a etiqueta GCM de 128 bits).
 */
class CifraAesGcm(private val chave: () -> SecretKey) {

    /** Cifra um texto (ou devolve `null` para `null`). Cada chamada usa um IV novo. */
    fun cifrar(texto: String?): String? {
        if (texto == null) return null
        val cipher = Cipher.getInstance(TRANSFORMACAO)
        cipher.init(Cipher.ENCRYPT_MODE, chave())
        val cifrado = cipher.doFinal(texto.toByteArray(Charsets.UTF_8))
        return PREFIXO + Base64.getEncoder().encodeToString(cipher.iv + cifrado)
    }

    /**
     * Decifra um valor guardado por [cifrar]. Um valor antigo em claro (sem o prefixo) é devolvido
     * como está; um valor alterado, corrompido ou cifrado com outra chave dá `null`.
     */
    fun decifrar(valor: String?): String? {
        if (valor == null) return null
        if (!valor.startsWith(PREFIXO)) return valor
        return try {
            val dados = Base64.getDecoder().decode(valor.removePrefix(PREFIXO))
            val cipher = Cipher.getInstance(TRANSFORMACAO)
            cipher.init(Cipher.DECRYPT_MODE, chave(), GCMParameterSpec(TAMANHO_ETIQUETA, dados, 0, TAMANHO_IV))
            String(cipher.doFinal(dados, TAMANHO_IV, dados.size - TAMANHO_IV), Charsets.UTF_8)
        } catch (e: Exception) {
            null
        }
    }

    companion object {
        const val PREFIXO = "enc1:"
        private const val TRANSFORMACAO = "AES/GCM/NoPadding"
        private const val TAMANHO_IV = 12
        private const val TAMANHO_ETIQUETA = 128
    }
}
