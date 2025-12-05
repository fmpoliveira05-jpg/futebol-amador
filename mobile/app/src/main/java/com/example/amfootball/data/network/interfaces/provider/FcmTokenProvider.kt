package com.example.amfootball.data.network.interfaces.provider

interface FcmTokenProvider {
    fun getDeviceToken(onResult: (String?) -> Unit)
}