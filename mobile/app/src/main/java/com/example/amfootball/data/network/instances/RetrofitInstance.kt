package com.example.amfootball.data.network.instances

import com.example.amfootball.data.network.AuthInterceptor
import com.example.amfootball.data.network.interfaces.AuthApi
import com.example.amfootball.data.network.interfaces.CalendarApi
import com.example.amfootball.data.network.interfaces.ChatApi
import com.example.amfootball.data.network.interfaces.LeadBoardApi
import com.example.amfootball.data.network.interfaces.MatchInviteApi
import com.example.amfootball.data.network.interfaces.PlayerApi
import com.example.amfootball.data.network.interfaces.TeamApi
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
object RetrofitInstance {

    // MUDE ISTO para o URL base da sua API .NET
    //private const val BASE_URL = "http:192.168.196.1" // link com ngrok
    private const val BASE_URL = "https://thrillful-temika-postlicentiate.ngrok-free.dev/"

    @Provides
    @Singleton
    fun provideOkHttpClient(): OkHttpClient {
        return OkHttpClient.Builder()
            .addInterceptor(AuthInterceptor())
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
    fun provideAuthApi(retrofit: Retrofit): AuthApi {
        return retrofit.create(AuthApi::class.java)
    }

    @Provides
    @Singleton
    fun provideCalendarApi(retrofit: Retrofit): CalendarApi {
        return retrofit.create(CalendarApi::class.java)
    }

    @Provides
    @Singleton
    fun provideChatApi(retrofit: Retrofit): ChatApi {
        return retrofit.create(ChatApi::class.java)
    }

    @Provides
    @Singleton
    fun provideLeadBoardApi(retrofit: Retrofit): LeadBoardApi {
        return retrofit.create(LeadBoardApi::class.java)
    }

    @Singleton
    @Provides
    fun provideMatchInviteApi(retrofit: Retrofit): MatchInviteApi {
        return retrofit.create(MatchInviteApi::class.java)
    }

    @Provides
    @Singleton
    fun providePlayerApi(retrofit: Retrofit): PlayerApi {
        return retrofit.create(PlayerApi::class.java)
    }

    @Provides
    @Singleton
    fun provideTeamApi(retrofit: Retrofit): TeamApi {
        return retrofit.create(TeamApi::class.java)
    }
}