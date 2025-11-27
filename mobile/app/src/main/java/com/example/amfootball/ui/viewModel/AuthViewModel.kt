package com.example.amfootball.ui.viewModel

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.UiState
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.repository.AuthRepository
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão da lógica de Autenticação (Login, Registo e Logout).
 *
 * Este ViewModel atua como intermediário entre a UI (Ecrãs de Login/Registo) e a camada de dados ([AuthRepository]).
 * Gere o estado de sessão do utilizador e executa operações assíncronas, notificando a UI através de callbacks e StateFlows.
 *
 * @property repository O repositório injetado que contém a lógica de negócio (Firebase + API + Sessão Local).
 */
@HiltViewModel
class AuthViewModel @Inject constructor(
    private val repository: AuthRepository
) : ViewModel() {

    /**
     * Estado interno mutável que indica se o utilizador tem uma sessão ativa.
     * Inicializado verificando se existe um token válido no armazenamento local.
     */
    private val _isUserLoggedIn = MutableStateFlow(repository.isUserLoggedIn())

    /**
     * Fluxo de estado público (apenas leitura) que a UI deve observar para determinar a navegação.
     * - `true`: O utilizador está autenticado (mostrar Home).
     * - `false`: O utilizador não está autenticado (mostrar Login).
     */
    val isUserLoggedIn = _isUserLoggedIn.asStateFlow()

    private val _uiState: MutableStateFlow<UiState> = MutableStateFlow(UiState(isLoading = false))
    val uiState: StateFlow<UiState> = _uiState
    /**
     * Executa o processo de login de forma assíncrona.
     *
     * Chama o repositório para autenticar no Firebase e guardar o token.
     * Se o login for bem-sucedido, atualiza o estado [_isUserLoggedIn] para `true`.
     *
     * @param email O email do utilizador.
     * @param password A palavra-passe do utilizador.
     * @param onResult Callback executado quando a operação termina. Recebe `true` se o login foi bem-sucedido, `false` caso contrário.
     */
    fun loginUser(email: String, password: String, onResult: (Boolean) -> Unit) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading =  true) }

            val success = repository.loginUser(email, password)

            if (success) {
                _isUserLoggedIn.value = true
            }

            _uiState.update { it.copy(isLoading = false, errorMessage = null) }
            onResult(success)
        }
    }

    /**
     * Termina a sessão do utilizador.
     *
     * Limpa os dados de autenticação no Firebase e no armazenamento local,
     * e atualiza o estado [_isUserLoggedIn] para `false`, forçando a UI a voltar ao ecrã de login.
     */
    fun logoutUser() {
        repository.logout()
        _isUserLoggedIn.value = false
    }

    /**
     * Regista um novo utilizador na aplicação.
     *
     * Coordena o processo de criação de conta, que envolve:
     * 1. Criar o utilizador no sistema de autenticação (Firebase).
     * 2. Enviar os detalhes do perfil ([CreateProfileDto]) para a API Backend.
     *
     * Se o registo for bem-sucedido, o utilizador é automaticamente considerado "logado".
     * Em caso de erro, a mensagem de exceção é passada para o callback [onError].
     *
     * @param profile DTO contendo os dados pessoais do utilizador (nome, idade, posição, etc.).
     * @param password A palavra-passe escolhida para a conta.
     * @param onSuccess Callback executado apenas se o registo for concluído com sucesso.
     * @param onError Callback executado se ocorrer alguma falha (rede, validação, etc.), fornecendo a mensagem de erro.
     */
    fun registerUser(
        profile: CreateProfileDto,
        password: String,
        onSuccess: () -> Unit,
        onError: (String) -> Unit
    ) {
        viewModelScope.launch {
            _uiState.update { it.copy(isLoading = true) }

            try {
                repository.registerUser(profile, password)

                _isUserLoggedIn.value = true

                onSuccess()
                _uiState.update { it.copy(isLoading = false, errorMessage = null) }
            } catch (e: Exception) {
                onError(e.message ?: "Erro desconhecido no registo")
                _uiState.update { it.copy(isLoading = false, errorMessage = e.message) }
            }
        }
    }
}