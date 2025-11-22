package com.example.amfootball.utils

import org.json.JSONObject

/**
 * Utilitários para lidar com o processamento de respostas de rede e extração de mensagens
 * de erro de APIs REST.
 *
 * Este objeto é crucial para padronizar o tratamento de erros HTTP não-sucesso (4xx/5xx).
 */
object NetworkUtils {

    /**
     * Tenta extrair uma mensagem de erro legível de um JSON de erro do Backend.
     *
     * Assume-se que o corpo do erro (errorBody) segue o padrão **RFC 7807 (Problem Details)**
     * (comum em APIs .NET/Spring Boot) e procura pelas chaves "detail" e "title".
     *
     * @param errorBody A string JSON crua ou texto não-JSON retornado por response.errorBody()?.string().
     * @return A mensagem de erro limpa (dando prioridade a "detail", depois a "title").
     * Retorna o [errorBody] original se não for um JSON válido, ou null se estiver vazio.
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
                else -> null
            }
        } catch (e: Exception) {
            errorBody
        }
    }
}