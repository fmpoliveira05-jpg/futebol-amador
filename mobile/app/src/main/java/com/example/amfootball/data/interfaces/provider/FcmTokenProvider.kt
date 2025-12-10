package com.example.amfootball.data.interfaces.provider

interface FcmTokenProvider {
    fun getDeviceToken(onResult: (String?) -> Unit)
}