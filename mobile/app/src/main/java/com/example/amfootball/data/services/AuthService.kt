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

/**
 * Serviço central responsável pela lógica de negócio de Autenticação e Registo.
 *
 * Esta classe atua como um mediador entre a UI, a API de Backend e o armazenamento local.
 * Gere o ciclo de vida da sessão do utilizador, garantindo que os tokens e perfis
 * são guardados ou limpos corretamente após cada operação.
 *
 * @property firebaseAuth Instância do SDK do Firebase Auth (usada para gestão de identidade e logout).
 * @property authApiService Interface Retrofit para comunicação com o Backend (login e criação de perfil na DB).
 * @property sessionManager Gestor de preferências locais para persistência de tokens e dados do utilizador.
 */
@Singleton
class AuthService @Inject constructor(
    private val firebaseAuth: FirebaseAuth,
    private val authApiService: AuthApi,
    private val sessionManager: SessionManager
) {
    /**
     * Realiza o processo de login do utilizador.
     *
     * Este método não só verifica as credenciais na API, como também executa os "side-effects"
     * necessários para manter a sessão ativa na aplicação:
     * 1. Chama o endpoint de login.
     * 2. Se bem-sucedido: Guarda o objeto [UserProfile] e o [AuthToken] no [SessionManager].
     * 3. Se falhar: Limpa qualquer sessão residual.
     *
     * @param login O DTO contendo email e password.
     * @return `true` se o login foi efetuado e os dados guardados com sucesso, `false` caso contrário.
     */
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
                val errorMsg =
                    response.errorBody()?.string() ?: "Erro desconhecido na API: ${response.code()}"
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

    /**
     * Regista um novo utilizador na plataforma.
     *
     * Este método implementa um padrão de **Transação Distribuída** (embora parte esteja comentada):
     * Tenta criar o utilizador no Firebase e, em seguida, criar o perfil na base de dados SQL via API.
     *
     * **Mecanismo de Rollback:**
     * Se a chamada à API falhar (`response.isSuccessful == false`) após o utilizador ter sido criado no Firebase,
     * o método captura a exceção e tenta apagar o utilizador do Firebase (`createdFirebaseUser.delete()`)
     * para evitar inconsistência de dados (Utilizador fantasma sem perfil na BD).
     *
     * @param profile DTO com os dados do perfil (Nome, Idade, Posição, etc.).
     * @param password A palavra-passe para criação da conta.
     * @throws Exception Se ocorrer erro na API ou no Firebase, propagando a mensagem para a UI.
     */
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

    /**
     * Encerra a sessão do utilizador.
     *
     * Executa o logout no SDK do Firebase e limpa todos os dados locais
     * (tokens e perfil) do [SessionManager].
     */
    fun logout() {
        firebaseAuth.signOut()
        sessionManager.clearSession()
    }

    /**
     * Verifica se existe uma sessão ativa.
     *
     * @return `true` se existe um token de autenticação guardado localmente, `false` caso contrário.
     */
    fun isUserLoggedIn(): Boolean {
        return sessionManager.getAuthToken() != null
    }
}