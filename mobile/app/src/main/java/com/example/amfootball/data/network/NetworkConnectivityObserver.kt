package com.example.amfootball.data.network

import android.content.Context
import android.net.ConnectivityManager
import android.net.Network
import android.net.NetworkCapabilities
import android.net.NetworkRequest
import dagger.hilt.android.qualifiers.ApplicationContext
import kotlinx.coroutines.channels.awaitClose
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.callbackFlow
import kotlinx.coroutines.flow.distinctUntilChanged
import kotlinx.coroutines.launch
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Classe responsável por monitorizar o estado da conexão à internet em tempo real.
 * Utiliza a API ConnectivityManager do Android e expõe os dados através de Kotlin Flows.
 */
@Singleton
class NetworkConnectivityObserver @Inject constructor(
    @ApplicationContext private val context: Context
) {

    private val connectivityManager =
        context.getSystemService(Context.CONNECTIVITY_SERVICE) as ConnectivityManager

    /**
     * Observa as mudanças de conectividade de rede em tempo real.
     *
     * Esta função cria um fluxo (Flow) que emite:
     * - [true]: Quando o dispositivo se conecta a uma rede válida (Wi-Fi ou Dados).
     * - [false]: Quando a conexão é perdida ou não existe.
     *
     * O fluxo é "distinctUntilChanged", ou seja, só emite valores se o estado mudar
     * (não emite "true" duas vezes seguidas).
     *
     * @return Um Flow<Boolean> que representa o estado da internet.
     */
    fun observeConnectivity(): Flow<Boolean> = callbackFlow {
        // Callback que recebe os eventos do sistema Android
        val callback = object : ConnectivityManager.NetworkCallback() {

            // Chamado quando uma nova rede é encontrada e está pronta para uso
            override fun onAvailable(network: Network) {
                super.onAvailable(network)
                launch { send(true) }
            }

            // Chamado quando a conexão com a rede é perdida
            override fun onLost(network: Network) {
                super.onLost(network)
                launch { send(false) } // Avisa: "Caímos"
            }

            // Chamado quando a rede não satisfaz os requisitos (sem internet)
            override fun onUnavailable() {
                super.onUnavailable()
                launch { send(false) }
            }
        }

        // Define os critérios para a rede que queremos (Internet + Wifi ou Celular)
        val networkRequest = NetworkRequest.Builder()
            .addCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
            .addTransportType(NetworkCapabilities.TRANSPORT_WIFI)
            .addTransportType(NetworkCapabilities.TRANSPORT_CELLULAR)
            .build()

        // Regista o callback no sistema Android para começar a escutar
        connectivityManager.registerNetworkCallback(networkRequest, callback)

        // Envia o estado inicial (para a UI saber logo se tem net ao abrir, sem esperar mudanças)
        val isCurrentlyConnected = isOnlineOneShot()
        trySend(isCurrentlyConnected)

        // Bloco executado quando o Flow é cancelado (ex: fechar a app ou sair da tela)
        // Essencial para evitar Memory Leaks (fugas de memória).
        awaitClose {
            connectivityManager.unregisterNetworkCallback(callback)
        }
    }.distinctUntilChanged()

    /**
     * Verifica o estado da internet instantaneamente (apenas uma vez).
     * Útil para obter o valor inicial antes de o callback disparar.
     */
    fun isOnlineOneShot(): Boolean {
        val network = connectivityManager.activeNetwork ?: return false
        val capabilities = connectivityManager.getNetworkCapabilities(network) ?: return false
        return capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
    }
}