package com.example.amfootball

import androidx.test.espresso.core.internal.deps.dagger.Module
import androidx.test.espresso.core.internal.deps.dagger.Provides
import com.example.amfootball.data.network.RetrofitInstance
import dagger.hilt.components.SingletonComponent
import dagger.hilt.testing.TestInstallIn
import okhttp3.OkHttpClient
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import javax.inject.Singleton

@Module
@TestInstallIn(
    components = [SingletonComponent::class],
    replaces = [RetrofitInstance::class]
)
object TestNetworkModule {

    @Provides
    @Singleton
    fun provideOkHttpClient(): OkHttpClient {
        // Cliente simples para testes, sem interceptors de Auth complexos
        return OkHttpClient.Builder().build()
    }

    @Provides
    @Singleton
    fun provideRetrofit(okHttpClient: OkHttpClient): Retrofit {
        return Retrofit.Builder()
            .baseUrl("http://localhost:8080/") //Endereço do MockWebServer
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create())
            .build()
    }

    @Provides
    @Singleton
    fun provideApiBackend(retrofit: Retrofit): ApiBackend {
        return retrofit.create(ApiBackend::class.java)
    }
}