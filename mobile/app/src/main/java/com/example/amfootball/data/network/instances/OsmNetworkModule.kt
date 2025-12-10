package com.example.amfootball.data.network.instances

import com.example.amfootball.data.network.interfaces.OpenStreetMapService
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import javax.inject.Singleton


@Module
@InstallIn(SingletonComponent::class)
object OsmNetworkModule {

    @Provides
    @Singleton
    fun provideOsmService(): OpenStreetMapService {
        return Retrofit.Builder()
            .baseUrl("https://nominatim.openstreetmap.org/")
            .addConverterFactory(GsonConverterFactory.create()) // Ou Moshi se estiveres a usar
            .build()
            .create(OpenStreetMapService::class.java)
    }
}