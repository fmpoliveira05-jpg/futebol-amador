package com.example.amfootball.utils

import org.json.JSONObject

/**
 * Utilitários para lidar com respostas de rede e erros de API.
 */
object NetworkUtils {
    /**
     * Tenta extrair uma mensagem de erro legível de um JSON de erro do Backend (.NET ProblemDetails).
     *
     * @param errorBody A string crua retornada pelo response.errorBody()?.string()
     * @return A mensagem de erro limpa ("detail" ou "title") ou null se falhar.
     */
    fun parseBackendError(errorBody: String?): String? {
        if (errorBody.isNullOrEmpty()) return null

        return try {
            val jsonObject = JSONObject(errorBody)
            val detail = if (jsonObject.has("detail")) jsonObject.getString("detail") else null
            val title = if (jsonObject.has("title")) jsonObject.getString("title") else null

            when {
                !detail.isNullOrEmpty() -> detail
                !title.isNullOrEmpty() -> title
                else -> null // É JSON, mas não tem as chaves que esperamos
            }
        } catch (e: Exception) {
            // Se não for JSON válido (ex: HTML de erro 500), retorna o texto original cru
            // ou null se preferires não mostrar HTML feio ao utilizador
            errorBody
        }
    }
}