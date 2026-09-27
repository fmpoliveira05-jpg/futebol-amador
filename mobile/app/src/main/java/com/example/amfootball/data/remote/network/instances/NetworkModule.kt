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
import android.content.Context
import com.example.amfootball.data.remote.network.AuthInterceptor
import com.example.amfootball.data.remote.network.IdempotenciaInterceptor
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import okhttp3.Cache
import java.io.File
import java.util.concurrent.TimeUnit
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
    /** Tempos máximos dos pedidos à API (segundos). */
    const val TEMPO_LIGACAO_S = 10L
    const val TEMPO_LEITURA_S = 20L
    const val TEMPO_ESCRITA_S = 20L
    const val TEMPO_TOTAL_S = 30L

    /** Cache HTTP em disco (10 MB), só para as respostas públicas que a API marca como cacheáveis. */
    const val TAMANHO_CACHE = 10L * 1024 * 1024

    /**
     * Providencia e configura o cliente HTTP [OkHttpClient]:
     * 1. tempos máximos de ligação, leitura, escrita e total ([TEMPO_TOTAL_S]);
     * 2. cache HTTP em disco para as respostas públicas;
     * 3. [AuthInterceptor] (token do Firebase) e [IdempotenciaInterceptor] (Idempotency-Key nos POST).
     */
    @Provides
    @Singleton
    fun provideOkHttpClient(
        authInterceptor: AuthInterceptor,
        idempotenciaInterceptor: IdempotenciaInterceptor,
        @ApplicationContext context: Context,
    ): OkHttpClient {
        return OkHttpClient.Builder()
            // Sem estes limites, um servidor que não responde deixa o ecrã a carregar para sempre;
            // o erro chega ao ecrã como "O servidor demorou demasiado a responder" (MensagensErro).
            .connectTimeout(TEMPO_LIGACAO_S, TimeUnit.SECONDS)
            .readTimeout(TEMPO_LEITURA_S, TimeUnit.SECONDS)
            .writeTimeout(TEMPO_ESCRITA_S, TimeUnit.SECONDS)
            .callTimeout(TEMPO_TOTAL_S, TimeUnit.SECONDS)
            // A API só deixa guardar as consultas públicas (ligas, classificação: "public, max-age=60");
            // as respostas com dados pessoais levam "no-store" e nunca vão para esta cache.
            .cache(Cache(File(context.cacheDir, "http"), TAMANHO_CACHE))
            .addInterceptor(authInterceptor)
            .addInterceptor(idempotenciaInterceptor)
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