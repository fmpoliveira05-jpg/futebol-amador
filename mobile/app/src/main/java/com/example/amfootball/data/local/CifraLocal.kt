package com.example.amfootball.data.local

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import android.util.Base64
import java.security.KeyStore
import javax.crypto.Cipher
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey
import javax.crypto.spec.GCMParameterSpec

/**
 * Cifra os dados da sessão guardados no telemóvel (token do Firebase e perfil, na base de dados
 * Room) com AES-256-GCM e uma chave do Android Keystore, que nunca sai do hardware seguro nem é
 * copiada em cópias de segurança.
 *
 * Formato guardado: `enc1:` + Base64(IV de 12 bytes + texto cifrado com a etiqueta GCM).
 * Valores antigos sem o prefixo (guardados em claro por versões anteriores) continuam a ser lidos
 * e ficam cifrados na próxima gravação. Se a chave desaparecer (reinstalação, restauro noutro
 * aparelho), [decifrar] devolve `null` e o utilizador só tem de voltar a entrar.
 */
object CifraLocal {
    private const val KEYSTORE = "AndroidKeyStore"
    private const val ALIAS = "amfootball_sessao"
    private const val TRANSFORMACAO = "AES/GCM/NoPadding"
    private const val PREFIXO = "enc1:"
    private const val TAMANHO_IV = 12
    private const val TAMANHO_ETIQUETA = 128

    @Synchronized
    private fun chave(): SecretKey {
        val keyStore = KeyStore.getInstance(KEYSTORE).apply { load(null) }
        (keyStore.getKey(ALIAS, null) as? SecretKey)?.let { return it }

        val gerador = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, KEYSTORE)
        gerador.init(
            KeyGenParameterSpec.Builder(ALIAS, KeyProperties.PURPOSE_ENCRYPT or KeyProperties.PURPOSE_DECRYPT)
                .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .setKeySize(256)
                .build()
        )
        return gerador.generateKey()
    }

    /** Cifra um texto (ou devolve `null` para `null`). */
    fun cifrar(texto: String?): String? {
        if (texto == null) return null
        val cipher = Cipher.getInstance(TRANSFORMACAO)
        cipher.init(Cipher.ENCRYPT_MODE, chave())
        val cifrado = cipher.doFinal(texto.toByteArray(Charsets.UTF_8))
        return PREFIXO + Base64.encodeToString(cipher.iv + cifrado, Base64.NO_WRAP)
    }

    /** Decifra um valor guardado por [cifrar]; um valor antigo em claro é devolvido tal como está. */
    fun decifrar(valor: String?): String? {
        if (valor == null) return null
        if (!valor.startsWith(PREFIXO)) return valor
        return try {
            val dados = Base64.decode(valor.removePrefix(PREFIXO), Base64.NO_WRAP)
            val cipher = Cipher.getInstance(TRANSFORMACAO)
            cipher.init(
                Cipher.DECRYPT_MODE,
                chave(),
                GCMParameterSpec(TAMANHO_ETIQUETA, dados, 0, TAMANHO_IV)
            )
            String(cipher.doFinal(dados, TAMANHO_IV, dados.size - TAMANHO_IV), Charsets.UTF_8)
        } catch (e: Exception) {
            null
        }
    }
}
