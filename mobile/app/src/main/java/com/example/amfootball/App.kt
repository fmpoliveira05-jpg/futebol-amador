package com.example.amfootball

import android.app.Application
import com.example.amfootball.data.local.SessionManager

class App : Application() {

    companion object {
        lateinit var sessionManager: SessionManager
            private set
    }

    override fun onCreate() {
        super.onCreate()
        sessionManager = SessionManager(applicationContext)
    }
}