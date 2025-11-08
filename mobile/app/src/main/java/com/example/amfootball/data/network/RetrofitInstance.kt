package com.example.amfootball.data.network

import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

object RetrofitInstance {

    // MUDE ISTO para o URL base da sua API .NET
    //private const val BASE_URL = "http:192.168.196.1" // link com ngrok
    private const val BASE_URL = "https://thrillful-temika-postlicentiate.ngrok-free.dev " // link com ngrok

    //Cria o cliente aqui
    private val okHttpClient = OkHttpClient.Builder()
        .addInterceptor(AuthInterceptor())
        // é possivel adicionar outros interceptors aqui
        .build()

    private val retrofit by lazy {
        Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(okHttpClient) // <-- 2. Dizer ao Retrofit para usar o nosso cliente
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    val api: ApiBackend by lazy {
        retrofit.create(ApiBackend::class.java)
    }
}