package com.example.amfootball.core.utils

import android.content.Context
import android.net.Uri
import com.cloudinary.android.MediaManager
import com.cloudinary.android.callback.ErrorInfo
import com.cloudinary.android.callback.UploadCallback

/**
 * Gerenciador responsável pela interação com o serviço de Cloudinary.
 *
 * Esta classe fornece métodos para inicializar a configuração do Cloudinary
 * e realizar o upload de imagens de forma assíncrona. Utiliza o padrão Singleton
 * ou Object para facilitar o acesso global na aplicação.
 *
 * */
object CloudinaryManager {

    /**
     * Indica se o MediaManager já foi inicializado para evitar reinicializações desnecessárias.
     */
    private var isInitialized = false

    /**
     * Inicializa a biblioteca do Cloudinary com as credenciais do projeto.
     *
     * Deve ser chamado apenas uma vez no ciclo de vida da aplicação, geralmente
     * na classe [android.app.Application] ou no onCreate da primeira Activity.
     *
     * @param context O contexto da aplicação necessário para inicializar o MediaManager.
     * @see MediaManager.init
     */
    fun init(context: Context) {
        if (!isInitialized) {
            val config = HashMap<String, String>()
            config["cloud_name"] = "o-teu-cloud-name"
            MediaManager.init(context, config)
            isInitialized = true
        }
    }

    /**
     * Realiza o upload de uma imagem para o Cloudinary utilizando um Preset não assinado (Unsigned).
     *
     * @param uri O URI do ficheiro de imagem a ser enviado (geralmente obtido via Picker ou Galeria).
     * @param onSuccess Callback invocado quando o upload é concluído com sucesso.
     * Recebe a URL pública (HTTPS) da imagem hospedada.
     * @param onError Callback invocado quando ocorre uma falha no upload.
     * Recebe uma String com a descrição do erro.
     *
     * @throws IllegalArgumentException Se o URI fornecido for inválido (gerido internamente pela lib).
     */
    fun uploadImage(
        uri: Uri,
        onSuccess: (String) -> Unit,
        onError: (String) -> Unit
    ) {
        val uploadPreset = "android_upload"

        MediaManager.get().upload(uri)
            .unsigned(uploadPreset)
            .option("resource_type", "image")
            .callback(object : UploadCallback {

                /**
                 * Chamado quando o upload inicia.
                 * @param requestId O ID único da requisição.
                 */
                override fun onStart(requestId: String) {
                    // Opcional: Iniciar loading na UI
                }

                /**
                 * Chamado durante o progresso do upload.
                 * @param requestId O ID único da requisição.
                 * @param bytes O número de bytes já enviados.
                 * @param totalBytes O tamanho total do ficheiro.
                 */
                override fun onProgress(requestId: String, bytes: Long, totalBytes: Long) {
                    // Opcional: Atualizar barra de progresso
                }

                /**
                 * Chamado quando o upload é concluído com sucesso.
                 * @param requestId O ID único da requisição.
                 * @param resultData Um mapa contendo os dados da resposta do servidor.
                 */
                override fun onSuccess(requestId: String, resultData: Map<*, *>) {
                    val publicUrl = resultData["secure_url"] as? String
                    if (publicUrl != null) {
                        onSuccess(publicUrl)
                    } else {
                        onError("Upload concluído, mas URL vazia.")
                    }
                }

                /**
                 * Chamado quando ocorre um erro no upload.
                 * @param requestId O ID único da requisição.
                 * @param errorInfo Informações detalhadas sobre o erro.
                 */
                override fun onError(requestId: String, errorInfo: ErrorInfo) {
                    onError(errorInfo.description)
                }

                /**
                 * Chamado quando o upload é reagendado (ex: perda de conexão).
                 * @param requestId O ID único da requisição.
                 * @param errorInfo Motivo do reagendamento.
                 */
                override fun onReschedule(requestId: String, errorInfo: ErrorInfo) {
                    // Lógica de retry automática da lib
                }
            })
            .dispatch()
    }
}