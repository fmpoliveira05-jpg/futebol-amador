package com.example.amfootball.data.services

import android.util.Log
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.dtos.player.LoginDto
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.interfaces.AuthApi
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.auth.FirebaseUser
import kotlinx.coroutines.tasks.await
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class AuthService @Inject constructor(
    private val firebaseAuth: FirebaseAuth,
    private val authApiService: AuthApi,
    private val sessionManager: SessionManager
) {
    suspend fun loginUser(login: LoginDto): Boolean {
        try {
            val response = authApiService.loginUser(login)
            if (response.isSuccessful && response.body() != null) {
                val userProfile = response.body()!!

                firebaseAuth.signInWithEmailAndPassword(login.email, login.password).await()


                sessionManager.saveUserProfile(userProfile)
                sessionManager.saveAuthToken(userProfile.loginResponseDto!!.idToken)
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

    suspend fun registerUser(profile: CreateProfileDto) {
        try {
            val response = authApiService.createProfile(profile)

            if (response.isSuccessful && response.body() != null) {
                val userProfile = response.body()!!

                sessionManager.saveUserProfile(userProfile)
                sessionManager.saveAuthToken(userProfile.loginResponseDto!!.idToken)
                Log.d("AuthRepository", "Login completo e dados guardados.")

            } else {
                val errorMsg = response.errorBody()?.string() ?: "Erro desconhecido na API: ${response.code()}"
                Log.e("AuthRepository", "Falha ao buscar perfil: $errorMsg")

                sessionManager.clearSession()
                throw Exception(errorMsg)
            }
        } catch (e: Exception) {
            println("Erro no registo: ${e.message}")
            sessionManager.clearSession()

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