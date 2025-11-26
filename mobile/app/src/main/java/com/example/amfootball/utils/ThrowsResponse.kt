package com.example.amfootball.utils

import retrofit2.Response

/**
 * Função genérica para tratar erros da API.
 * Recebe qualquer Response, extrai a mensagem de erro e lança a exceção.
 */
fun <T> handleApiError(response: Response<T>): Nothing {
    val errorRaw = response.errorBody()?.string()
    val errorMsg = NetworkUtils.parseBackendError(errorRaw)
        ?: "Erro desconhecido: ${response.code()}"

    throw Exception(errorMsg)
}