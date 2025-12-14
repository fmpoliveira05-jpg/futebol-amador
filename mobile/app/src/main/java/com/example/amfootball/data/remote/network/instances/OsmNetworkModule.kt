package com.example.amfootball.data.remote.network.instances

import com.example.amfootball.data.interfaces.services.OpenStreetMapService
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import javax.inject.Singleton

/**
 * Módulo Hilt que fornece a instância do serviço Retrofit para interagir com a API
 * Nominatim do OpenStreetMap (OSM).
 *
 * Utiliza o [SingletonComponent] para garantir que o serviço é injetado como um Singleton
 * com ciclo de vida da aplicação, otimizando recursos e conexões.
 */
@Module
@InstallIn(SingletonComponent::class)
object OsmNetworkModule {

    /**
     * Fornece uma instância única (Singleton) do [OpenStreetMapService].
     *
     * O Retrofit é configurado com a URL base da API Nominatim para operações de geocodificação.
     *
     * @return Uma implementação do [OpenStreetMapService] pronta a usar (serviço de geocodificação).
     */
    @Provides
    @Singleton
    fun provideOsmService(): OpenStreetMapService {
        return Retrofit.Builder()
            .baseUrl("https://nominatim.openstreetmap.org/")
            .addConverterFactory(GsonConverterFactory.create())
            .build()
            .create(OpenStreetMapService::class.java)
    }
}