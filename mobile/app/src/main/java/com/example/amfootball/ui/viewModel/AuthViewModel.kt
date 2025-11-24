package com.example.amfootball.ui.viewModel

import android.util.Log
import androidx.lifecycle.ViewModel
import kotlinx.coroutines.tasks.await
import com.example.amfootball.App
import com.example.amfootball.data.dtos.CreateProfileDto
import com.example.amfootball.data.network.RetrofitInstance
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.auth.FirebaseUser


class AuthViewModel : ViewModel() {
    private val firebaseAuth = FirebaseAuth.getInstance()
    private val apiService = RetrofitInstance.api

    private val sessionManager = App.sessionManager
    suspend fun loginUser(email: String, password: String): Boolean {
        try {
            // fazer login no Firebase
            val authResult = firebaseAuth.signInWithEmailAndPassword(email, password).await()

            val firebaseUser = authResult.user
            if (firebaseUser == null) {
                throw Exception("Utilizador Firebase não encontrado após login.")
            }

            // token JWT do utilizador
            val idTokenResult = firebaseUser.getIdToken(true).await()
            val token = idTokenResult.token

            if (token.isNullOrEmpty()) {
                throw Exception("Não foi possível obter o token do Firebase.")
            }

            Log.d("TOKEN_TEST", "Bearer $token")

            // guarda o token localmente
            sessionManager.saveAuthToken(token)

            // busca os dados na api backend (descomentar quando a rota estiver pronta)

             //val profile = apiService.getMyProfile("Bearer $token")
            // sessionManager.saveUserProfile(profile)

            return true // Login bem-sucedido

        } catch (e: Exception) {
            // Tratamento de erros
            println("Erro no login: ${e.message}")
            return false // Login falhou
        }
    }

    fun logoutUser() {
        firebaseAuth.signOut()
        sessionManager.clearSession()
    }

    /**
     * Regista um novo utilizador.
     * Comporta-se da seguinte forma:
     * - Se for bem-sucedida, o user é criado no Firebase E no backend.
     * - Se falhar a meio, faz um rollback para limpar os dados.
     *
     * @throws Exception se o registo falhar em qualquer ponto.
     */
    suspend fun registerUser(profile: CreateProfileDto, password: String) {
        var createdFirebaseUser: FirebaseUser? = null
        try {
            // Tenta criar o utilizador no Firebase
            //val authResult = firebaseAuth.createUserWithEmailAndPassword(profile.email, password).await()
            /*
            createdFirebaseUser = authResult.user
            if (createdFirebaseUser == null) {
                // Lançar erro, registo falhou
                throw Exception("Utilizador Firebase não foi criado.")
            }

            // Obtem o token JWT do utilizador
            //pos forceRefresh = true garante que obtemos um token fresco
            val idTokenResult = createdFirebaseUser.getIdToken(true).await()
            val token = idTokenResult.token

            if (token.isNullOrEmpty()) {
                throw Exception("Não foi possível obter o token do Firebase.")
            }

            Log.d("TOKEN_TEST", "Bearer $token")
            sessionManager.saveAuthToken(token)

            */

            // criar o perfil no backend
            val response = apiService.createProfile( profile)

            if (!response.isSuccessful) {
                throw Exception("Falha ao criar perfil: ${response.code()}")
            }

        } catch (e: Exception) {
            println("Erro no registo: ${e.message}")
            sessionManager.clearSession()

            if (createdFirebaseUser != null) {
                try {
                    createdFirebaseUser.delete().await()
                    Log.d("AuthViewModel", "Rollback: Utilizador ${createdFirebaseUser.uid} apagado do Firebase.")
                } catch (deleteEx: Exception) {
                    Log.e("AuthViewModel", "CRÍTICO: Falha ao apagar user do Firebase durante o rollback. Erro: ${deleteEx.message}")
                }
                throw Exception("Erro no registo: ${e.message}")
            }
        }
    }
}