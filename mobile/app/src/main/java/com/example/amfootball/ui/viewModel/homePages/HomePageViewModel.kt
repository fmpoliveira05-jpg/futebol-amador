package com.example.amfootball.ui.viewModel.homePages

import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.local.SessionManager
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.ui.viewModel.abstracts.BaseViewModel
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

//TODO: Falta validações de autorização
/**
 * ViewModel responsável pela lógica da Home Page Principal (Dashboard do Utilizador).
 *
 * Este ViewModel gere os dados do perfil do utilizador logado e controla a navegação
 * para as funcionalidades principais (Criar Equipa, Ver Pedidos, Listar Equipas).
 *
 * Funcionalidades principais:
 * 1. Carregamento do perfil do utilizador a partir da sessão local.
 * 2. Validação de segurança (Autenticação) antes de permitir a navegação.
 * 3. Validação de conectividade (Internet) para funcionalidades que requerem API.
 *
 * @property networkObserver Observador de conectividade para validações de rede.
 * @property sessionManager Gestor de sessão para recuperar o perfil e tokens do utilizador.
 */
@HiltViewModel
class HomePageViewModel @Inject constructor(
    private val networkObserver: NetworkConnectivityObserver,
    private val sessionManager: SessionManager
): BaseViewModel(networkObserver = networkObserver, needObserverNetwork = true) {
    /**
     * Estado interno mutável que armazena os dados do perfil do jogador.
     * Inicializado como `null` até que os dados sejam carregados da sessão.
     */
    private val userData: MutableStateFlow<PlayerProfileDto?> = MutableStateFlow(null)

    /**
     * Fluxo público imutável (Read-only) contendo os dados do utilizador.
     * A UI observa este fluxo para exibir o nome do utilizador, foto e decidir
     * quais cartões de ação mostrar (ex: se tem equipa ou não).
     */
    val user: StateFlow<PlayerProfileDto?> = userData.asStateFlow()

    init {
        loadUserData()
    }

    /**
     * Tenta navegar para o ecrã de "Criar Equipa".
     *
     * Realiza as seguintes validações:
     * 1. Verifica se o utilizador possui um token de autenticação válido.
     *
     * Se falhar, exibe um Toast de erro. Se passar, executa o [onSuccessNavigation].
     *
     * @param onSuccessNavigation Callback de navegação a ser executada se as validações passarem.
     */
    fun onNavigateCreateTeam(onSuccessNavigation: () -> Unit) {
        if (sessionManager.getAuthToken() == null) {
            updateToast(message = "Precisa de estar autenticado para aceder a esta funcionalidade")
            return
        }

        onSuccessNavigation()
    }

    /**
     * Tenta navegar para a lista de "Pedidos de Adesão".
     *
     * Realiza as seguintes validações:
     * 1. Verifica se o [idPlayer] é válido.
     * 2. Verifica se existe token de autenticação.
     * 3. **Segurança:** Garante que o ID solicitado corresponde ao ID do utilizador logado na sessão.
     * 4. **Rede:** Verifica se há conexão à internet (necessário para buscar pedidos à API).
     *
     * @param idPlayer O ID do jogador para o qual se quer ver os pedidos.
     * @param onSuccessNavigation Callback de navegação a ser executada se tudo estiver válido.
     */
    fun onNavigationToRequests(idPlayer: String?, onSuccessNavigation: () -> Unit) {
        if (idPlayer.isNullOrBlank() || sessionManager.getAuthToken() == null){
            updateToast(message = "Precisa de estar autenticado para aceder a esta funcionalidade")
            return
        }

        if (idPlayer != sessionManager.getUserProfile()?.loginResponseDto?.localId) {
            updateToast(message = "O id enviado não é o mesmo do seu")
            return
        }

        onlineFunctionality(
            action = {
                onSuccessNavigation()
            },
            toastMessage = "Precisa de estar conectado, para visualiza os pedidos de adesão recebidos"
        )
    }

    /**
     * Tenta navegar para a "Lista de Equipas".
     *
     * Realiza as seguintes validações:
     * 1. **Rede:** Verifica se há conexão à internet (necessário para buscar a lista à API).
     *
     * @param onSuccessNavigation Callback de navegação.
     */
    fun onNavigateToListTeams(onSuccessNavigation: () -> Unit) {
        onlineFunctionality(
            action = { onSuccessNavigation() },
            toastMessage = "Precisa de estar conectado, para visualiza a lista de equipas"
        )
    }

    /**
     * Carrega os dados do perfil do utilizador armazenados localmente no [SessionManager].
     *
     * Utiliza [launchDataLoad] com `checkOnline = false` porque esta operação é
     * estritamente local (SharedPreferences/DataStore) e deve funcionar mesmo offline.
     */
    private fun loadUserData() {
        launchDataLoad (
            checkOnline = false,
            callApi = { userData.value = sessionManager.getUserProfile() }
        )
    }
}