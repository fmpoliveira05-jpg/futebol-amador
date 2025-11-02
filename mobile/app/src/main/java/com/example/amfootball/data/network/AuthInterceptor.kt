package com.example.amfootball.data.network

import com.example.amfootball.App
import okhttp3.Interceptor
import okhttp3.Response

class AuthInterceptor : Interceptor {

    // Obtém o sessionManager estático da classe App
    private val sessionManager = App.sessionManager

    override fun intercept(chain: Interceptor.Chain): Response {
        val originalRequest = chain.request()

        val token = sessionManager.getAuthToken()

        if (token.isNullOrEmpty()) {
            return chain.proceed(originalRequest)
        }

        val newRequestBuilder = originalRequest.newBuilder()
            .header("Authorization", "Bearer $token")

        return chain.proceed(newRequestBuilder.build())
    }
}