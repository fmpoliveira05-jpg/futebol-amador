package com.example.amfootball.data.remote.services

import com.example.amfootball.core.utils.safeApiCallWithReturn
import com.example.amfootball.data.interfaces.api.AuthApi
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.remote.dtos.player.CreateProfileDto
import com.example.amfootball.data.remote.dtos.player.LoginDto
import com.google.firebase.auth.FirebaseAuth
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
            firebaseAuth.signInWithEmailAndPassword(login.email, login.password).await()
            val userProfile = safeApiCallWithReturn {
                authApiService.loginUser(login)
            }

            firebaseAuth.signInWithEmailAndPassword(login.email, login.password).await()

            sessionManager.saveUserProfile(userProfile)
            sessionManager.saveAuthToken(userProfile.loginResponseDto!!.idToken)
            return true
        } catch (e: Exception) {
            e.printStackTrace()
            return false
        }
    }

    /**
     * Regista um novo utilizador na plataforma.
     *
     * A API cria a conta no Firebase e o perfil na base de dados, e devolve logo a sessão.
     * Se falhar, a sessão local é limpa e o erro é propagado para o ecrã o mostrar.
     *
     * @param profile DTO com os dados do perfil (Nome, Idade, Posição, etc.).
     * @throws Exception Se ocorrer erro na API ou no Firebase, propagando a mensagem para a UI.
     */
    suspend fun registerUser(profile: CreateProfileDto) {
        try {
            val response = authApiService.createProfile(profile)

            if (response.isSuccessful && response.body() != null) {
                val userProfile = response.body()!!

                sessionManager.saveUserProfile(userProfile)
                sessionManager.saveAuthToken(userProfile.loginResponseDto!!.idToken)
            } else {
                val errorMsg =
                    response.errorBody()?.string() ?: "Erro desconhecido na API: ${response.code()}"

                sessionManager.clearSession()
                throw Exception(errorMsg)
            }
        } catch (e: Exception) {
            // Antes o erro era engolido aqui: o ecrã de registo julgava que tinha corrido bem e
            // avançava sem sessão. Agora limpa-se a sessão e o erro chega ao ViewModel.
            sessionManager.clearSession()
            throw e
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