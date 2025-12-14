package com.example.amfootball.data.remote.network.instances

import com.example.amfootball.data.interfaces.services.ImageUploadService
import com.example.amfootball.data.services.FirebaseImageUploadService
import dagger.Binds
import dagger.Module
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

@Module
@InstallIn(SingletonComponent::class)
abstract class ServiceModule {

    @Binds
    @Singleton
    abstract fun bindImageUploadService(
        impl: FirebaseImageUploadService
    ): ImageUploadService
}