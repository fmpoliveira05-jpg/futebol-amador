package com.example.amfootball.core.utils

import android.content.Context
import com.example.amfootball.BuildConfig
import com.cloudinary.android.MediaManager

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
            // Definido em local.properties (CLOUDINARY_CLOUD_NAME), fora do repositório.
            config["cloud_name"] = BuildConfig.CLOUDINARY_CLOUD_NAME
            MediaManager.init(context, config)
            isInitialized = true
        }
    }

    // O upload passou a ser assinado pela API: ver data/services/CloudinaryUploader.kt.
}
