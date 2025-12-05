package com.example.amfootball.mockWebServer

import com.example.amfootball.data.network.instances.FireBaseInstance
import com.example.amfootball.data.network.interfaces.provider.FcmTokenProvider
import com.google.firebase.Firebase
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.firestore.FirebaseFirestore
import com.google.firebase.firestore.firestore
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

    @Provides
    @Singleton
    fun provideFirestore(): FirebaseFirestore {
        return Firebase.firestore
    }

    @Provides
    @Singleton
    fun provideFcmTokenProvider(): FcmTokenProvider {
        return object : FcmTokenProvider {
            override fun getDeviceToken(onResult: (String?) -> Unit) {
                onResult("token-teste-123456")
            }
        }
    }
}