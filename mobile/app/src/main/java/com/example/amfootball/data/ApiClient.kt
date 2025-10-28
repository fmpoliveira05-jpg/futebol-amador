package com.example.amfootball.data

import android.util.Log
import com.example.amfootball.data.Dtos.CreateRoomRequest
import com.example.amfootball.data.Interfaces.ApiBackend
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory

// Usar "object" cria um Singleton. Só haverá UMA instância do ApiClient.
object ApiClient {

    private const val BASE_URL = "http://10.0.2.2:5218"

    // 1. Criar a instância principal do Retrofit (lazy para criar só quando for usado)
    private val retrofit: Retrofit by lazy {
        Retrofit.Builder()
            .baseUrl(BASE_URL)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    // 2. Criar a instância do serviço de API que pode ser chamada de qualquer lado
    // Ex: ApiClient.service.createChatRoom(...)
    val service: ApiBackend by lazy {
        retrofit.create(ApiBackend::class.java)
    }



    fun chamarApiDotNet(token: String) {
        // Use Coroutines para a chamada de rede
        GlobalScope.launch(Dispatchers.IO) {
            val requestBody = CreateRoomRequest(
                roomName = "Sala de Teste do Kotlin",
                teamIds = listOf("team-A", "team-C")
            )

            try {
                val response = ApiClient.service.createChatRoom(
                    token = "Bearer $token",
                    request = requestBody
                )

                if (response.isSuccessful) {
                    val roomResponse = response.body()
                    Log.d("ApiTeste", "Sala criada com sucesso! ID: ${roomResponse?.roomId}")
                } else {
                    Log.e("ApiTeste", "Falha na API: ${response.code()}")
                    Log.e("ApiTeste", "Erro: ${response.errorBody()?.string()}")
                }
            } catch (e: Exception) {
                Log.e("ApiTeste", "Erro de rede: ${e.message}", e)
            }
        }
    }
}
