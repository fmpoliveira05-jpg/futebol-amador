package com.example.amfootball.data.remote.network.instances

import com.example.amfootball.core.utils.NetworkConsts
import com.example.amfootball.data.interfaces.api.AuthApi
import com.example.amfootball.data.interfaces.api.CalendarApi
import com.example.amfootball.data.interfaces.api.ChatApi
import com.example.amfootball.data.interfaces.api.LeadBoardApi
import com.example.amfootball.data.interfaces.api.MatchInviteApi
import com.example.amfootball.data.interfaces.api.NotificationApi
import com.example.amfootball.data.interfaces.api.PlayerApi
import com.example.amfootball.data.interfaces.api.PostPoneMatchApi
import com.example.amfootball.data.interfaces.api.TeamApi
import com.example.amfootball.data.remote.network.AuthInterceptor
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import javax.inject.Singleton

/**
 * Módulo de Rede (NetworkModule) para configuração de dependências de conectividade.
 *
 * Este objeto centraliza a configuração de **toda a conectividade** da aplicação,
 * incluindo chamadas REST (Retrofit) e gerenciamento de conexões em tempo real (SignalR).
 *
 * É instalado no [SingletonComponent], garantindo que as instâncias providenciadas
 * (como o Retrofit e o OkHttpClient) são únicas durante todo o ciclo de vida da aplicação.
 */
@Module
@InstallIn(SingletonComponent::class)
object NetworkModule {
    /**
     * Providencia e configura o cliente HTTP [OkHttpClient].
     *
     * Este cliente é configurado com:
     * 1. Um interceptor de autenticação ([AuthInterceptor]) para injetar o token JWT nos pedidos.
     * 2. Um interceptor customizado para adicionar o header `ngrok-skip-browser-warning`,
     * necessário para evitar crashes de parsing de JSON devido à página de aviso do Ngrok.
     *
     * @param authInterceptor A instância do interceptor de autenticação injetada.
     * @return Uma instância configurada e [Singleton] de [OkHttpClient].
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
            .baseUrl(NetworkConsts.BASE_URL)
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    /**
     * Fornece a implementação da interface [AuthApi] para chamadas de autenticação (Login/Registo).
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [AuthApi].
     */
    @Provides
    @Singleton
    fun provideAuthApi(retrofit: Retrofit): AuthApi {
        return retrofit.create(AuthApi::class.java)
    }

    /**
     * Fornece a implementação da interface [CalendarApi] para gestão de eventos e jogos.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [CalendarApi].
     */
    @Provides
    @Singleton
    fun provideCalendarApi(retrofit: Retrofit): CalendarApi {
        return retrofit.create(CalendarApi::class.java)
    }

    /**
     * Fornece a implementação da interface [ChatApi] para chamadas de chat e mensagens.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [ChatApi].
     */
    @Provides
    @Singleton
    fun provideChatApi(retrofit: Retrofit): ChatApi {
        return retrofit.create(ChatApi::class.java)
    }

    /**
     * Fornece a implementação da interface [LeadBoardApi] para consulta de classificações.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [LeadBoardApi].
     */
    @Provides
    @Singleton
    fun provideLeadBoardApi(retrofit: Retrofit): LeadBoardApi {
        return retrofit.create(LeadBoardApi::class.java)
    }

    /**
     * Fornece a implementação da interface [MatchInviteApi] para gestão de convites de jogo.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [MatchInviteApi].
     */
    @Singleton
    @Provides
    fun provideMatchInviteApi(retrofit: Retrofit): MatchInviteApi {
        return retrofit.create(MatchInviteApi::class.java)
    }

    /**
     * Fornece a implementação da interface [PlayerApi] para gestão de perfis de jogadores.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [PlayerApi].
     */
    @Provides
    @Singleton
    fun providePlayerApi(retrofit: Retrofit): PlayerApi {
        return retrofit.create(PlayerApi::class.java)
    }

    /**
     * Fornece a implementação da interface [TeamApi] para gestão de equipas.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [TeamApi].
     */
    @Provides
    @Singleton
    fun provideTeamApi(retrofit: Retrofit): TeamApi {
        return retrofit.create(TeamApi::class.java)
    }

    /**
     * Fornece a implementação da interface [PostPoneMatchApi] para gestão de pedidos de adiamento de jogos.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [PostPoneMatchApi].
     */
    @Provides
    @Singleton
    fun providePostPoneMatchApi(retrofit: Retrofit): PostPoneMatchApi {
        return retrofit.create(PostPoneMatchApi::class.java)
    }

    /**
     * Fornece a implementação da interface [NotificationApi] para atualização do Device Token FCM.
     * @param retrofit Instância base do Retrofit.
     * @return Implementação da [NotificationApi].
     */
    @Provides
    @Singleton
    fun provideNotificationApi(retrofit: Retrofit): NotificationApi {
        return retrofit.create(NotificationApi::class.java)
    }
}