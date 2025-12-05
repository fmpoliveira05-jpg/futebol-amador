package com.example.amfootball

import android.app.Application
import com.example.amfootball.data.local.SessionManager
import dagger.hilt.android.HiltAndroidApp

/**
 * Classe principal da aplicação (Application Class).
 *
 * É o ponto de entrada da aplicação Android e o host do Dagger Hilt.
 *
 * **Responsabilidades Principais:**
 * 1. Inicializar o Dagger Hilt (marcado por [@HiltAndroidApp]).
 * 2. Gerir a inicialização e o acesso estático (Global Access) a serviços essenciais,
 * como o [SessionManager].
 */
@HiltAndroidApp
class App : Application() {

    companion object {
        /**
         * Acesso estático (Singleton Global) ao gestor de sessão da aplicação.
         *
         * **Nota:** O uso de `lateinit var` e acesso estático (Service Locator Pattern) é
         * geralmente desaconselhado em arquiteturas com Hilt, pois viola o princípio da Injeção
         * de Dependências. Deve ser usado apenas para utilitários críticos de nível de aplicação.
         * */
        lateinit var sessionManager: SessionManager
            private set
    }

    /**
     * Chamado quando a aplicação é criada pela primeira vez (antes de qualquer Activity).
     * */
    override fun onCreate() {
        super.onCreate()
        sessionManager = SessionManager(applicationContext)
    }
}