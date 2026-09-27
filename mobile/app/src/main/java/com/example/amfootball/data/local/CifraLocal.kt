package com.example.amfootball.data.local

import android.security.keystore.KeyGenParameterSpec
import android.security.keystore.KeyProperties
import java.security.KeyStore
import javax.crypto.KeyGenerator
import javax.crypto.SecretKey

/**
 * Cifra os dados da sessão guardados no telemóvel (token do Firebase e perfil, na base de dados
 * Room) com AES-256-GCM e uma chave do Android Keystore, que nunca sai do hardware seguro nem é
 * copiada em cópias de segurança.
 *
 * Formato guardado: `enc1:` + Base64(IV de 12 bytes + texto cifrado com a etiqueta GCM), ver [CifraAesGcm].
 * Valores antigos sem o prefixo (guardados em claro por versões anteriores) continuam a ser lidos
 * e ficam cifrados na próxima gravação. Se a chave desaparecer (reinstalação, restauro noutro
 * aparelho), [decifrar] devolve `null` e o utilizador só tem de voltar a entrar.
 */
object CifraLocal {
    private const val KEYSTORE = "AndroidKeyStore"
    private const val ALIAS = "amfootball_sessao"

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

    private val cifra = CifraAesGcm(::chave)

    /** Cifra um texto (ou devolve `null` para `null`). */
    fun cifrar(texto: String?): String? = cifra.cifrar(texto)

    /** Decifra um valor guardado por [cifrar]; um valor antigo em claro é devolvido tal como está. */
    fun decifrar(valor: String?): String? = cifra.decifrar(valor)
}
