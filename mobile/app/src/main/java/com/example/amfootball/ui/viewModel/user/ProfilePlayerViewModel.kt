package com.example.amfootball.ui.viewModel.user

import androidx.lifecycle.SavedStateHandle
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.UiState
import com.example.amfootball.data.dtos.player.PlayerProfileDto
import com.example.amfootball.data.repository.PlayerRepository
import com.example.amfootball.utils.NetworkUtils
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão de estado e lógica de negócio do ecrã de Perfil de Jogador.
 *
 * Esta classe gere o ciclo de vida dos dados do perfil, incluindo:
 * - Obtenção do ID do jogador através dos argumentos de navegação.
 * - Gestão dos estados de UI (Loading, Sucesso, Erro) através de [UiState].
 * - Comunicação com o [PlayerRepository] para obter dados da API.
 *
 * @property savedStateHandle O manipulador de estado guardado para recuperar argumentos de navegação (ex: playerId).
 * @property repository O repositório responsável por fornecer os dados do jogador.
 */
@HiltViewModel
class ProfilePlayerViewModel @Inject constructor(
    private val savedStateHandle: SavedStateHandle,
    private val repository: PlayerRepository
) : ViewModel() {

    /**
     * Fluxo de dados que contém os detalhes do perfil do jogador ([PlayerProfileDto]).
     * A UI observa este fluxo para apresentar as informações (nome, foto, estatísticas, etc.).
     * Inicialmente é nulo até que os dados sejam carregados com sucesso.
     */
    private val profilePlayer: MutableStateFlow<PlayerProfileDto?> = MutableStateFlow(null)
    val uiProfilePlayer: StateFlow<PlayerProfileDto?> = profilePlayer.asStateFlow()

    /**
     * O ID do jogador recuperado dos argumentos da rota de navegação.
     * Se for nulo, assume-se que é o perfil do utilizador autenticado (sessão atual).
     */
    val playerId: String? = savedStateHandle["playerId"]

    /**
     * Fluxo de estado da UI que encapsula o estado de carregamento (loading) e mensagens de erro.
     * Inicia com `isLoading = true` pois a tentativa de carregamento começa no `init`.
     */
    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = true))
    val uiState: StateFlow<UiState> = _uiState

    init {
        if (playerId != null) {
            // Carrega o perfil de outro jogador (ID vindo da navegação)
            loadPlayerProfile(playerId = playerId)
        } else {
            // Carrega o perfil do próprio utilizador (Sessão)
            // TODO: AQUI NÃO CHAMA ISTO DEPOIS, CHAMA É O SESSION_MANAGER
            loadPlayerProfile(playerId = "iIbMFBATjAYjPWu5dC8ezoEyzw12")
        }
    }

    fun retry() {
        val id = playerId ?: "iIbMFBATjAYjPWu5dC8ezoEyzw12"
        loadPlayerProfile(playerId = id)
    }

    /**
     * Carrega os dados do perfil do jogador da API de forma assíncrona.
     *
     * Este método atualiza o [_uiState] para refletir o progresso:
     * 1. Define `isLoading = true`.
     * 2. Faz a chamada ao repositório.
     * 3. Em caso de sucesso: Atualiza [profilePlayer] com os dados e remove o loading.
     * 4. Em caso de erro (API ou Rede): Define `isLoading = false` e preenche `errorMessage`.
     *
     * @param playerId O ID único do jogador a ser carregado.
     */
    private fun loadPlayerProfile(playerId: String) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true, errorMessage = null) }
            try {
                val response = repository.getPlayerProfile(playerId = playerId)

                if (response.isSuccessful && response.body() != null) {
                    profilePlayer.value = response.body()

                    _uiState.update { it.copy(isLoading = false) }
                } else {
                    val errorRaw = response.errorBody()?.string()
                    val errorMsg = NetworkUtils.parseBackendError(errorRaw)
                        ?: "Erro desconhecido: ${response.code()}"

                    _uiState.update {
                        it.copy(isLoading = false, errorMessage = errorMsg)
                    }
                }
            } catch (e: Exception) {
                _uiState.update {
                    it.copy(isLoading = false, errorMessage = "Sem conexão: ${e.localizedMessage}")
                }
            }
        }
    }
}