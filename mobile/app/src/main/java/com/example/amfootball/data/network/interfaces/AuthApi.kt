package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.dtos.player.LoginDto
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.POST

/**
 * Interface de API responsável pela Autenticação e Gestão do Perfil do Utilizador.
 *
 * Esta interface define os endpoints Retrofit relacionados com o ciclo de vida da conta do utilizador,
 * incluindo:
 * - Sincronização do registo (Firebase -> Backend SQL).
 * - Obtenção dos dados do próprio perfil (MyProfile).
 * - Processo de Login no sistema.
 *
 * Faz parte da segregação da API principal para garantir o princípio de responsabilidade única.
 */
interface AuthApi {
    /**
     * Sincroniza o perfil criado no Firebase com a base de dados SQL Server do Backend.
     * Deve ser chamado logo após o registo inicial.
     *
     * Endpoint: POST api/Player/create-profile
     *
     * @param request O DTO contendo os dados iniciais do perfil do utilizador.
     * @return [Response] vazia (Unit) indicando sucesso (200 OK) ou falha.
     */
    @POST("${BaseEndpoints.PLAYER_API}/create-profile")
    suspend fun createProfile(
        @Body request: CreateProfileDto
    ): Response<PlayerProfileDto>

    /**
     * Realiza a autenticação do utilizador no sistema (Login).
     *
     * Este endpoint valida as credenciais fornecidas. Se a autenticação for bem-sucedida,
     * a resposta incluirá o perfil do utilizador, permitindo o início da sessão na aplicação.
     *
     * Endpoint: POST {BaseEndpoints.authApi}/login
     *
     * @param request O DTO [LoginDto] contendo as credenciais de login (email e password/token).
     * @return [Response] contendo o perfil detalhado do utilizador [PlayerProfileDto] em caso de sucesso.
     */
    @POST("${BaseEndpoints.AUTH_API}/login")
    suspend fun loginUser(
        @Body request: LoginDto
    ): Response<PlayerProfileDto>
}