package com.example.amfootball.data.network.instances

import com.example.amfootball.data.network.AuthInterceptor
import com.example.amfootball.data.network.interfaces.AuthApi
import com.example.amfootball.data.network.interfaces.CalendarApi
import com.example.amfootball.data.network.interfaces.ChatApi
import com.example.amfootball.data.network.interfaces.LeadBoardApi
import com.example.amfootball.data.network.interfaces.MatchInviteApi
import com.example.amfootball.data.network.interfaces.PlayerApi
import com.example.amfootball.data.network.interfaces.TeamApi
import com.google.firebase.auth.FirebaseAuth
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import io.reactivex.rxjava3.core.Single
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import javax.inject.Singleton

/**
 * Módulo principal de configuração de Rede (Network) utilizando Dagger Hilt.
 *
 * Este objeto centraliza a criação e fornecimento de todas as instâncias relacionadas
 * com a comunicação HTTP da aplicação, incluindo o cliente [OkHttpClient],
 * o construtor [Retrofit] e as implementações das interfaces de API.
 */
@Module
@InstallIn(SingletonComponent::class)
object RetrofitInstance {

    /**
     * URL base da API de Backend.
     *
     * Atualmente configurado para um túnel **Ngrok** (HTTPS), permitindo expor o localhost
     * da máquina de desenvolvimento para a internet pública e ser acessível pelo dispositivo Android.
     */
    private const val BASE_URL = "https://thrillful-temika-postlicentiate.ngrok-free.dev/"

    /**
     * Providencia e configura o cliente HTTP [OkHttpClient].
     *
     * Este cliente inclui dois interceptores cruciais:
     * 1. [AuthInterceptor]: Injeta o token de autenticação nos pedidos.
     * 2. **Interceptor Anónimo (Ngrok):** Adiciona o header `ngrok-skip-browser-warning`.
     * Isto é necessário porque o Ngrok gratuito apresenta uma página HTML de aviso
     * antes de deixar passar o pedido, o que quebraria o parsing de JSON da app (Crash).
     *
     * @param authInterceptor A instância do interceptor de autenticação injetada automaticamente pelo Hilt.
     * O Hilt sabe como criar isto porque o AuthInterceptor tem um construtor @Inject.
     *
     * @return Uma instância configurada de [OkHttpClient].
     */
    @Provides
    @Singleton
    fun provideOkHttpClient(
        authInterceptor: AuthInterceptor
    ): OkHttpClient {
        return OkHttpClient.Builder()
            .addInterceptor(authInterceptor)
            .addInterceptor { chain ->
                val original = chain.request()
                val request = original.newBuilder()
                    .header("ngrok-skip-browser-warning", "true")
                    .method(original.method, original.body)
                    .build()
                chain.proceed(request)
            }
            .build()
    }

    /**
     * Providencia a instância do [Retrofit] configurada.
     *
     * @param okHttpClient O cliente HTTP configurado acima, injetado automaticamente pelo Hilt.
     * @return O objeto Retrofit pronto para criar implementações de APIs, configurado com
     * [GsonConverterFactory] para serialização JSON.
     */
    @Provides
    @Singleton
    fun provideRetrofit(okHttpClient: OkHttpClient): Retrofit {
        return Retrofit.Builder()
            .baseUrl(BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    @Provides
    @Singleton
    fun provideSignalRManager(): SignalRManager {
        return SignalRManager(
            baseUrl = BASE_URL,
            tokenProvider = {
                Single.create { emitter ->
                    val user = FirebaseAuth.getInstance().currentUser

                    if (user != null) {
                        user.getIdToken(false)
                            .addOnSuccessListener { result ->
                                // Sucesso: Enviamos APENAS o token (sem "Bearer")
                                val token = result.token ?: ""
                                emitter.onSuccess(token)
                            }
                            .addOnFailureListener { exception ->
                                emitter.onError(exception)
                            }
                    } else {
                        emitter.onError(Exception("Utilizador não autenticado no Firebase"))
                    }
                }
            }
        )
    }

    /**
     * Fornece a implementação da API de Autenticação.
     */
    @Provides
    @Singleton
    fun provideAuthApi(retrofit: Retrofit): AuthApi {
        return retrofit.create(AuthApi::class.java)
    }

    /**
     * Fornece a implementação da API de Calendário de Jogos.
     */
    @Provides
    @Singleton
    fun provideCalendarApi(retrofit: Retrofit): CalendarApi {
        return retrofit.create(CalendarApi::class.java)
    }

    /**
     * Fornece a implementação da API de Chat e Mensagens.
     */
    @Provides
    @Singleton
    fun provideChatApi(retrofit: Retrofit): ChatApi {
        return retrofit.create(ChatApi::class.java)
    }

    /**
     * Fornece a implementação da API de Classificações (Leaderboard).
     */
    @Provides
    @Singleton
    fun provideLeadBoardApi(retrofit: Retrofit): LeadBoardApi {
        return retrofit.create(LeadBoardApi::class.java)
    }

    /**
     * Fornece a implementação da API de Convites de Jogo.
     */
    @Singleton
    @Provides
    fun provideMatchInviteApi(retrofit: Retrofit): MatchInviteApi {
        return retrofit.create(MatchInviteApi::class.java)
    }

    /**
     * Fornece a implementação da API de Gestão de Jogadores.
     */
    @Provides
    @Singleton
    fun providePlayerApi(retrofit: Retrofit): PlayerApi {
        return retrofit.create(PlayerApi::class.java)
    }

    /**
     * Fornece a implementação da API de Gestão de Equipas.
     */
    @Provides
    @Singleton
    fun provideTeamApi(retrofit: Retrofit): TeamApi {
        return retrofit.create(TeamApi::class.java)
    }
}