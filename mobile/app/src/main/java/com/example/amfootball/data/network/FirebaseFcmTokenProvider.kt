package com.example.amfootball.data.network

import com.example.amfootball.data.network.interfaces.provider.FcmTokenProvider
import com.google.firebase.messaging.FirebaseMessaging
import javax.inject.Inject

class FirebaseFcmTokenProvider @Inject constructor() : FcmTokenProvider {
    override fun getDeviceToken(onResult: (String?) -> Unit) {
        try {
            FirebaseMessaging.getInstance().token.addOnCompleteListener { task ->
                if (task.isSuccessful) {
                    onResult(task.result)
                } else {
                    onResult(null)
                }
            }
        } catch (e: Exception) {
            onResult(null)
        }
    }
}