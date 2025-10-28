package com.example.amfootball.viewModel
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope // Importante
import com.google.firebase.auth.auth
import com.google.firebase.Firebase
import kotlinx.coroutines.launch
import kotlinx.coroutines.tasks.await // Importante
import android.util.Log
import com.example.amfootball.data.ApiClient.chamarApiDotNet

class LoginViewModel : ViewModel() {
    private val auth = Firebase.auth

    fun loginParaTestarAPI(email: String, password: String) {
        // Use o viewModelScope para lançar uma Coroutine
        viewModelScope.launch {
            try {
                // 1. FAZER LOGIN (Forma moderna com Coroutines)
                val authResult = auth.signInWithEmailAndPassword(email, password).await()
                Log.d("LoginTeste", "Login bem-sucedido!")

                // 2. OBTER O TOKEN
                val user = authResult.user
                val tokenResult = user?.getIdToken(true)?.await()
                val token = tokenResult?.token

                if (token != null) {
                    Log.d("LoginTeste", "--- TOKEN DO FIREBASE ---")
                    Log.d("LoginTeste", token)
                    Log.d("LoginTeste", "-------------------------")

                    // 3. ENVIAR O TOKEN
                    chamarApiDotNet(token)
                }
            } catch (e: Exception) {
                // Falha no login ou ao obter token
                Log.w("LoginTeste", "Falha no processo de login:", e)
            }
        }
    }
}