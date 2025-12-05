package com.example.amfootball.utils

import retrofit2.Response

/**
 * Processa uma resposta de erro da API e lança a exceção correspondente.
 *
 * Esta função extrai a mensagem de erro do corpo da resposta (`errorBody`),
 * tenta fazer o parse (caso o backend envie JSON de erro estruturado) e interrompe
 * o fluxo de execução lançando uma [Exception].
 *
 * @param response A resposta Retrofit que contém o código de erro (HTTP 4xx ou 5xx).
 * @return [Nothing] - Esta função nunca retorna um valor, pois garante o lançamento de uma exceção.
 * @throws Exception Com a mensagem de erro vinda do backend ou uma genérica se o parse falhar.
 */
fun <T> handleApiError(response: Response<T>): Nothing {
    val errorRaw = response.errorBody()?.string()
    val errorMsg = NetworkUtils.parseBackendError(errorRaw)
        ?: "Erro desconhecido: ${response.code()}"

    throw Exception(errorMsg)
}