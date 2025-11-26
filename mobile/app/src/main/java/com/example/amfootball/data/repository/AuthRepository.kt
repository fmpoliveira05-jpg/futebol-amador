package com.example.amfootball.data.repository

import android.util.Log
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.dtos.player.LoginDto
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.ApiBackend
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.auth.FirebaseUser
import kotlinx.coroutines.tasks.await
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AuthRepository @Inject constructor(
    private val firebaseAuth: FirebaseAuth,
    private val apiService: ApiBackend,
    private val sessionManager: SessionManager
) {
    suspend fun loginUser(email: String, password: String): Boolean {
        try {
/*
            val authResult = firebaseAuth.signInWithEmailAndPassword(email, password).await()


            val firebaseUser = authResult.user

            if (firebaseUser == null) {
                throw Exception("Utilizador Firebase não encontrado após login.")
            }

            val idTokenResult = firebaseUser.getIdToken(true).await()
            val token = idTokenResult.token

            if (token.isNullOrEmpty()) {
                throw Exception("Não foi possível obter o token do Firebase.")
            }

            Log.d("TOKEN_TEST", "Bearer $token")


*/
            val login  = LoginDto(
                email = email,
                password = password
            )
            val response = apiService.loginUser(login)

            if (response.isSuccessful && response.body() != null) {
                val userProfile = response.body()!!

                sessionManager.saveUserProfile(userProfile)
                sessionManager.saveAuthToken(userProfile.loginResponseDto.idToken)
                Log.d("AuthRepository", "Login completo e dados guardados.")

                return true
            } else {
                val errorMsg = response.errorBody()?.string() ?: "Erro desconhecido na API: ${response.code()}"
                Log.e("AuthRepository", "Falha ao buscar perfil: $errorMsg")

                sessionManager.clearSession()
                throw Exception(errorMsg)
            }
        } catch (e: Exception) {
            Log.e("AuthRepository", "Erro no login: ${e.message}")
            e.printStackTrace()
            return false
        }
    }

    suspend fun registerUser(profile: CreateProfileDto, password: String) {
        val createdFirebaseUser: FirebaseUser? = null

        try {
            /*
            val authResult = firebaseAuth.createUserWithEmailAndPassword(profile.email, password).await()
            createdFirebaseUser = authResult.user

            if (createdFirebaseUser == null) {
                throw Exception("Utilizador Firebase não foi criado.")
            }


            val idTokenResult = createdFirebaseUser.getIdToken(true).await()
            val token = idTokenResult.token

            if (token.isNullOrEmpty()) {
                throw Exception("Não foi possível obter o token do Firebase.")
            }

            Log.d("TOKEN_TEST", "Bearer $token")

            sessionManager.saveAuthToken(token)
*/

            val response = apiService.createProfile(profile)

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

    fun logout() {
        firebaseAuth.signOut()
        sessionManager.clearSession()
    }

    fun isUserLoggedIn(): Boolean {
        return sessionManager.getAuthToken() != null
    }
}