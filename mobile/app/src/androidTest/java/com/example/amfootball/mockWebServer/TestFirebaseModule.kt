package com.example.amfootball.mockWebServer

import com.example.amfootball.data.network.instances.FireBaseInstance
import com.google.firebase.auth.FirebaseAuth
import dagger.Module
import dagger.Provides
import dagger.hilt.components.SingletonComponent
import dagger.hilt.testing.TestInstallIn
import org.mockito.Mockito
import javax.inject.Singleton

@Module
@TestInstallIn(
    components = [SingletonComponent::class],
    replaces = [FireBaseInstance::class]
)
object TestFirebaseModule {
    @Provides
    @Singleton
    fun provideFirebaseAuth(): FirebaseAuth {
        return Mockito.mock(FirebaseAuth::class.java)
    }
}