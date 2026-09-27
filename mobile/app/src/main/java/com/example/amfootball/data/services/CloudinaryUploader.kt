package com.example.amfootball.data.services

import android.content.Context
import android.net.Uri
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaTypeOrNull
import okhttp3.MultipartBody
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import org.json.JSONObject
import retrofit2.Response
import retrofit2.Retrofit
import retrofit2.http.POST
import javax.inject.Inject
import javax.inject.Singleton

/** Resposta do `POST api/uploads/signature` (ver UploadsController na API). */
data class AssinaturaUploadDto(
    val uploadUrl: String = "",
    val apiKey: String = "",
    val timestamp: Long = 0,
    val signature: String = "",
    val folder: String = "",
    val uploadPreset: String = "",
    val allowedFormats: String = "",
    val maxBytes: Long = 0
)

/** Endpoint da API que assina os uploads. */
interface UploadsApi {
    @POST("api/uploads/signature")
    suspend fun assinatura(): Response<AssinaturaUploadDto>
}

/**
 * Upload assinado de imagens para o Cloudinary.
 *
 * Antes a app usava um preset não assinado (`android_upload`): quem soubesse o nome da conta podia
 * enviar ficheiros. Agora a API (com o `api_secret`, que nunca está na app) assina cada upload para
 * a pasta do utilizador, o preset assinado e os formatos JPG, PNG e WebP; a app só envia o ficheiro
 * com esses parâmetros. O pedido ao Cloudinary não leva o token da API (cliente OkHttp próprio).
 *
 * NOTA: não verificado neste ambiente (sem Android SDK) — compilar e testar a criação de equipa
 * com emblema.
 */
@Singleton
class CloudinaryUploader @Inject constructor(
    @ApplicationContext private val context: Context,
    retrofit: Retrofit
) {
    private val api: UploadsApi = retrofit.create(UploadsApi::class.java)
    private val http = OkHttpClient()

    /**
     * Envia a imagem e devolve o URL HTTPS guardado no Cloudinary.
     * @throws Exception se a API não assinar, o ficheiro for grande demais ou o upload falhar.
     */
    suspend fun enviar(uri: Uri): String = withContext(Dispatchers.IO) {
        val resposta = api.assinatura()
        val assinatura = resposta.body()
        if (!resposta.isSuccessful || assinatura == null) {
            throw Exception("Não foi possível preparar o envio da imagem (${resposta.code()}).")
        }

        val tipo = context.contentResolver.getType(uri) ?: "image/jpeg"
        val bytes = context.contentResolver.openInputStream(uri)?.use { it.readBytes() }
            ?: throw Exception("Não foi possível ler a imagem.")
        if (assinatura.maxBytes > 0 && bytes.size > assinatura.maxBytes) {
            throw Exception("A imagem é demasiado grande (máximo ${assinatura.maxBytes / (1024 * 1024)} MB).")
        }

        // Os campos têm de ser exatamente os que a API assinou.
        val corpo = MultipartBody.Builder()
            .setType(MultipartBody.FORM)
            .addFormDataPart("file", "emblema", bytes.toRequestBody(tipo.toMediaTypeOrNull()))
            .addFormDataPart("api_key", assinatura.apiKey)
            .addFormDataPart("timestamp", assinatura.timestamp.toString())
            .addFormDataPart("signature", assinatura.signature)
            .addFormDataPart("folder", assinatura.folder)
            .addFormDataPart("upload_preset", assinatura.uploadPreset)
            .addFormDataPart("allowed_formats", assinatura.allowedFormats)
            .build()

        http.newCall(Request.Builder().url(assinatura.uploadUrl).post(corpo).build()).execute().use { r ->
            val texto = r.body?.string().orEmpty()
            if (!r.isSuccessful) {
                throw Exception("O Cloudinary recusou a imagem (${r.code}).")
            }
            JSONObject(texto).optString("secure_url").takeIf { it.startsWith("https://") }
                ?: throw Exception("Upload concluído, mas sem URL.")
        }
    }
}
