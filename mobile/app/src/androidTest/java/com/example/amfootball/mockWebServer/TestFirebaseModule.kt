package com.example.amfootball.mockWebServer


import com.example.amfootball.data.network.interfaces.provider.FcmTokenProvider
import com.google.android.gms.tasks.Tasks
import com.google.firebase.auth.AuthResult
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.auth.FirebaseUser
import com.google.firebase.firestore.FirebaseFirestore
import org.mockito.ArgumentMatchers.anyString
import org.mockito.Mockito

object TestFirebaseModule {

    fun provideFirebaseAuth(): FirebaseAuth {
        val mockAuth = Mockito.mock(FirebaseAuth::class.java)
        val mockAuthResult = Mockito.mock(AuthResult::class.java)
        val mockUser = Mockito.mock(FirebaseUser::class.java)
        val successTask = Tasks.forResult(mockAuthResult)

        Mockito.`when`(mockAuth.signInWithEmailAndPassword(anyString(), anyString()))
            .thenReturn(successTask)

        Mockito.`when`(mockAuth.currentUser).thenReturn(mockUser)

        return mockAuth
    }

    fun provideFirestore(): FirebaseFirestore {
        return Mockito.mock(FirebaseFirestore::class.java)
    }

    fun provideFcmTokenProvider(): FcmTokenProvider {
        return object : FcmTokenProvider {
            override fun getDeviceToken(onResult: (String?) -> Unit) {
                onResult("token-teste-mock")
            }
        }
    }
}