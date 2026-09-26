package com.example.amfootball.competicao

import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import retrofit2.Retrofit
import javax.inject.Singleton

/**
 * Fornece a [CompeticaoApi]. Fica num módulo à parte para funcionar tanto com o NetworkModule
 * da app como com o módulo de rede dos testes instrumentados (que só troca o Retrofit).
 */
@Module
@InstallIn(SingletonComponent::class)
object CompeticaoModule {
    @Provides
    @Singleton
    fun provideCompeticaoApi(retrofit: Retrofit): CompeticaoApi =
        retrofit.create(CompeticaoApi::class.java)
}
