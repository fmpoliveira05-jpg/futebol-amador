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
 * Componente responsável pela monitorização reativa do estado da conexão à internet.
 *
 * Esta classe atua como uma abstração sobre o [ConnectivityManager] do sistema Android,
 * convertendo os callbacks de sistema baseados em eventos num fluxo contínuo de dados ([Flow])
 * consumível de forma assíncrona pela aplicação.
 *
 * **Características principais:**
 * - Deteta mudanças entre Wi-Fi, Dados Móveis e Sem Rede.
 * - Valida a existência real de internet (não apenas a conexão ao router).
 * - Gere automaticamente o ciclo de vida dos callbacks para evitar fugas de memória.
 *
 * @property context O contexto da aplicação injetado via Hilt ([ApplicationContext]),
 * necessário para aceder aos serviços de sistema.
 */
@Singleton
class NetworkConnectivityObserver @Inject constructor(
    @ApplicationContext private val context: Context
) {

    /**
     * Instância do serviço de sistema [ConnectivityManager].
     * Utilizado para registar callbacks de rede e consultar as capacidades ativas.
     */
    private val connectivityManager =
        context.getSystemService(Context.CONNECTIVITY_SERVICE) as ConnectivityManager

    /**
     * Observa as mudanças de conectividade de rede em tempo real.
     *
     * Utiliza um [callbackFlow] para converter os eventos do [ConnectivityManager.NetworkCallback]
     * num fluxo "Cold" (frio) de booleanos.
     *
     * **Comportamento do Fluxo:**
     * 1. **Emissão Inicial:** Assim que o fluxo é coletado, emite imediatamente o estado atual (`isOnlineOneShot`).
     * 2. **Atualizações:** Sempre que o sistema deteta uma alteração (perda de sinal, troca de Wi-Fi para Dados, etc.),
     * o novo estado é verificado e emitido.
     * 3. **Filtragem:** Utiliza [distinctUntilChanged] para evitar emissões repetidas do mesmo estado
     * (ex: evitar múltiplos `true` se a rede mudar de Wi-Fi A para Wi-Fi B mas a internet se mantiver).
     * 4. **Limpeza:** Quando o coletor (Coroutine) é cancelado, o bloco [awaitClose] garante
     * que o callback é removido do sistema, prevenindo Memory Leaks.
     *
     * **Eventos Monitorizados:**
     * - [NetworkCallback.onAvailable]: Rede detetada e pronta.
     * - [NetworkCallback.onLost]: Rede perdida.
     * - [NetworkCallback.onCapabilitiesChanged]: As propriedades da rede mudaram (ex: perdeu acesso à internet
     * mas manteve a ligação ao router).
     * - [NetworkCallback.onUnavailable]: Nenhuma rede satisfaz os requisitos.
     *
     * @return Um [Flow] que emite `true` se houver acesso validado à internet, e `false` caso contrário.
     */
    fun observeConnectivity(): Flow<Boolean> = callbackFlow {
        // Lança uma corrotina para enviar o estado atualizado para o fluxo.
        val sendCurrentState = {
            launch { send(isOnlineOneShot()) }
        }

        // Definição do callback que escuta os eventos do sistema Android
        val callback = object : ConnectivityManager.NetworkCallback() {

            /**
             * Disparado quando uma nova rede satisfaz os critérios definidos no [networkRequest].
             * Indica que o dispositivo estabeleceu uma conexão.
             */
            override fun onAvailable(network: Network) {
                super.onAvailable(network)
                sendCurrentState()
            }

            /**
             * Disparado quando as capacidades da rede mudam.
             * Crítico para detetar situações onde o utilizador desliga os dados móveis manualmente,
             * ou entra numa rede Wi-Fi que requer login (Captive Portal), perdendo a validação de internet.
             */
            override fun onCapabilitiesChanged(
                network: Network,
                networkCapabilities: NetworkCapabilities
            ) {
                super.onCapabilitiesChanged(network, networkCapabilities)
                sendCurrentState()
            }

            /**
             * Disparado quando a conexão à rede é totalmente interrompida.
             */
            override fun onLost(network: Network) {
                super.onLost(network)
                sendCurrentState()
            }

            /**
             * Disparado quando a rede não é encontrada ou não satisfaz os requisitos mínimos.
             */
            override fun onUnavailable() {
                super.onUnavailable()
                sendCurrentState()
            }
        }

        //Configuração do Request: Estamos interessados em qualquer rede que possua capacidade de INTERNET
        val networkRequest = NetworkRequest.Builder()
            .addCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
            .build()

        //Registo do callback no sistema
        connectivityManager.registerNetworkCallback(networkRequest, callback)

        //Envia o estado inicial para que a UI não fique num estado "indefinido" até à primeira mudança de rede
        val isCurrentlyConnected = isOnlineOneShot()
        trySend(isCurrentlyConnected)

        // Suspende a corrotina até que o fluxo seja fechado ou cancelado.
        // No momento do cancelamento, remove o callback para libertar recursos do sistema.
        awaitClose {
            connectivityManager.unregisterNetworkCallback(callback)
        }
    }.distinctUntilChanged()

    /**
     * Realiza uma verificação síncrona e pontual do estado da conectividade.
     *
     * Verifica a rede ativa no momento e valida se esta possui as capacidades necessárias.
     *
     * **Critérios de Validação:**
     * 1. Existe uma [ConnectivityManager.activeNetwork]?
     * 2. Essa rede possui [NetworkCapabilities.NET_CAPABILITY_INTERNET]? (Tem acesso à rede mundial)
     * 3. Essa rede possui [NetworkCapabilities.NET_CAPABILITY_VALIDATED]? (O sistema confirmou que a conexão é funcional e não está bloqueada por um portal cativo)
     *
     * @return `true` apenas se todas as condições forem satisfeitas, `false` caso contrário.
     */
    fun isOnlineOneShot(): Boolean {
        val network = connectivityManager.activeNetwork ?: return false
        val capabilities = connectivityManager.getNetworkCapabilities(network) ?: return false
        return capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_INTERNET)
                && capabilities.hasCapability(NetworkCapabilities.NET_CAPABILITY_VALIDATED)

    }
}