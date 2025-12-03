package com.example.amfootball.utils

import retrofit2.Response

/**
 * Executa uma chamada de API de forma segura, tratando exceções e erros HTTP centralizadamente.
 *
 * Esta função reduz a repetição de código (boilerplate) nos Serviços/Repositórios,
 * encapsulando os blocos try-catch e a validação de `response.isSuccessful`.
 *
 * @param apiCall Uma função suspensa (lambda) que retorna um [Response] do Retrofit.
 * @return O corpo da resposta (Body) do tipo [T] se o pedido for bem-sucedido.
 * @throws Exception Se a resposta for de erro ou ocorrer uma falha de rede.
 */
suspend fun <T> safeApiCallWithReturn(apiCall: suspend () -> Response<T>): T {
    try {
        val response = apiCall()

        if (response.isSuccessful) {
            val body = response.body()

            if (body != null) {
                return body
            } else {
                throw Exception("A API respondeu com sucesso, mas o corpo está vazio (null).")
            }
        } else {
            handleApiError(response)
        }
    } catch (e: Exception) {
        e.printStackTrace()
        throw e
    }
}

/**
 * Executa uma chamada de API onde o retorno de dados é irrelevante ou inexistente (Void/Unit).
 *
 * Este método é ideal para operações de "Fire and Forget" (ex: POST, PUT, DELETE, PATCH)
 * onde apenas interessa saber se a operação foi bem-sucedida (HTTP 2xx),
 * ignorando o conteúdo do corpo da resposta (que pode ser vazio/null).
 *
 * @param apiCall Função suspensa que retorna [Response] de qualquer tipo.
 * @throws Exception Apenas se a resposta for de erro (HTTP 4xx/5xx) ou falha de rede.
 */
suspend fun <T> safeApiCallWithNotReturn(apiCall: suspend () -> Response<T>) {
    try {
        val response = apiCall()

        if (!response.isSuccessful) {
            handleApiError(response)
        }
    } catch (e: Exception) {
        e.printStackTrace()
        throw e
    }
}