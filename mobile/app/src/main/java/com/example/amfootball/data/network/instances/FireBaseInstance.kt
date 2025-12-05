package com.example.amfootball.data.network.instances

import com.example.amfootball.data.network.FirebaseFcmTokenProvider
import com.example.amfootball.data.network.interfaces.provider.FcmTokenProvider
import com.google.firebase.Firebase
import com.google.firebase.auth.FirebaseAuth
import com.google.firebase.firestore.FirebaseFirestore
import com.google.firebase.firestore.firestore
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.components.SingletonComponent
import javax.inject.Singleton

/**
 * Módulo de Injeção de Dependências (Dagger Hilt) responsável por fornecer instâncias do SDK do Firebase.
 *
 * Este objeto atua como uma fábrica de dependências para os serviços da Google, garantindo que
 * classes como Repositórios ou Data Sources recebam as instâncias prontas a usar via construtor,
 * em vez de dependerem de chamadas estáticas (ex: `getInstance()`).
 *
 * A anotação [@InstallIn(SingletonComponent::class)] garante que as dependências aqui definidas
 * estão disponíveis em toda a aplicação e persistem durante todo o seu ciclo de vida.
 */
@Module
@InstallIn(SingletonComponent::class)
object FireBaseInstance {

    /**
     * Providencia a instância padrão do serviço de autenticação [FirebaseAuth].
     *
     * A anotação [@Singleton] garante que apenas uma única instância do [FirebaseAuth] é criada
     * e reutilizada em todos os pontos de injeção da aplicação, otimizando recursos.
     *
     * **Benefício de Arquitetura:**
     * Ao injetar o [FirebaseAuth] em vez de usar `FirebaseAuth.getInstance()` diretamente nas classes,
     * torna-se possível substituir esta instância por um "Mock" durante os testes unitários,
     * simulando logins e erros sem contactar o servidor real.
     *
     * @return A instância ativa do gestor de autenticação do Firebase.
     */
    @Provides
    @Singleton
    fun provideFirebaseAuth(): FirebaseAuth {
        return FirebaseAuth.getInstance()
    }
    @Provides
    @Singleton
    fun provideFirestore(): FirebaseFirestore {
        return FirebaseFirestore.getInstance()
    }

    @Provides
    @Singleton
    fun provideFcmTokenProvider(): FcmTokenProvider {
        return FirebaseFcmTokenProvider()
    }
}