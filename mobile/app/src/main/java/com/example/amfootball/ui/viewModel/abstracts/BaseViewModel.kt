package com.example.amfootball.ui.viewModel.abstracts

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.UiState
import com.example.amfootball.data.network.NetworkConnectivityObserver
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch

/**
 * ViewModel Base Abstrato.
 *
 * Serve como fundação para todos os ViewModels da aplicação. Centraliza a lógica comum de:
 * 1. Gestão de Estado da UI ([com.example.amfootball.data.UiState]) para Loading, Erros e Mensagens (Toasts).
 * 2. Monitorização de Conectividade de Rede em tempo real.
 * 3. Execução segura de chamadas assíncronas (Corrotinas) com tratamento de exceções automático.
 *
 * @property networkObserver Dependência para monitorizar o estado da internet.
 */
abstract class BaseViewModel(
    private val networkObserver: NetworkConnectivityObserver
): ViewModel() {
    /**
     * Estado global da UI.
     * Contém informações sobre se o ecrã está a carregar ([com.example.amfootball.data.UiState.isLoading]),
     * se ocorreu algum erro ([com.example.amfootball.data.UiState.errorMessage]) ou se há mensagens para exibir ([com.example.amfootball.data.UiState.toastMessage]).
     */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState.asStateFlow()

    /**
     * Estado de conectividade em tempo real.
     * True se houver internet, False caso contrário.
     * Este fluxo é atualizado automaticamente pelo [observeNetworkChanges].
     */
    private val _isOnline = MutableStateFlow(networkObserver.isOnlineOneShot())
    val isOnline: StateFlow<Boolean> = _isOnline.asStateFlow()

    init {
        // Inicia a observação da rede assim que qualquer ViewModel filho é instanciado.
        observeNetworkChanges()
    }

    /**
     * Função auxiliar para executar operações assíncronas (ex: chamadas à API) de forma segura e padronizada.
     *
     * Automatiza o seguinte fluxo:
     * 1. Define `isLoading = true` e limpa erros antigos.
     * 2. (Opcional) Verifica se há internet. Se não houver, define mensagem de erro e aborta.
     * 3. Executa o bloco de código [block] dentro de um `try-catch`.
     * 4. Se sucesso: Define `isLoading = false`.
     * 5. Se erro (Exception): Captura a exceção, imprime o stacktrace e define a mensagem de erro na UI.
     *
     * @param checkOnline Se `true` (padrão), impede a execução se o dispositivo estiver offline.
     * @param callApi A função suspensa (lambda) que contém a lógica de negócio ou chamada à API.
     */
    protected fun launchDataLoad(checkOnline: Boolean = true, callApi: suspend () -> Unit) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }

            if (checkOnline && !networkObserver.isOnlineOneShot()) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem internet. Verifique a sua conexão.")
                }
                return@launch
            }

            try {
                callApi()

                _uiState.update { it.copy(isLoading = false) }
            } catch (e: Exception) {
                e.printStackTrace()
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Erro: ${e.localizedMessage}")
                }
            }
        }
    }

    /**
     * Define uma mensagem temporária (Toast) para ser exibida na UI.
     * Útil para feedbacks rápidos que não bloqueiam o ecrã.
     *
     * @param message A mensagem a exibir.
     */
    protected fun updateToast(message: String?) {
        _uiState.update { it.copy(toastMessage = message) }
    }

    /**
     * Método de "Consumo" do Toast.
     * Deve ser chamado pela UI (Composable/Fragment) imediatamente após o Toast ser exibido.
     * Isto limpa a mensagem do estado para evitar que o Toast apareça repetidamente ao rodar o ecrã.
     */
    fun onToastShown() {
        _uiState.update { it.copy(toastMessage = null) }
    }

    /**
     * Helper para verificar a conectividade de forma síncrona (One-Shot).
     * Útil para lógica condicional dentro dos ViewModels filhos (ex: if (isNetworkAvailable()) ...).
     *
     * @return `true` se estiver online, `false` se estiver offline.
     */
    protected fun isNetworkAvailable() = networkObserver.isOnlineOneShot()

    /**
     * Inicia a recolha do Flow de conectividade do [NetworkConnectivityObserver].
     * Mantém o [isOnline] atualizado enquanto o ViewModel estiver vivo.
     */
    private fun observeNetworkChanges() {
        viewModelScope.launch {
            networkObserver.observeConnectivity().collect { status ->
                _isOnline.value = status
            }
        }
    }
}