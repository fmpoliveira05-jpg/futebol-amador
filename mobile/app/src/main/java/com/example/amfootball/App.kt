package com.example.amfootball

import android.app.Application
import com.example.amfootball.core.utils.CloudinaryManager
import com.example.amfootball.data.local.SessionManager
import dagger.hilt.android.HiltAndroidApp
import javax.inject.Inject

@HiltAndroidApp
class App : Application() {
    @Inject
    lateinit var hiltSessionManager: SessionManager

    companion object {
        /**
         * Mantemos o acesso estático para compatibilidade com o resto do código.
         */
        lateinit var sessionManager: SessionManager
            private set
    }

    override fun onCreate() {
        super.onCreate()
        sessionManager = hiltSessionManager

        CloudinaryManager.init(this)
    }
}