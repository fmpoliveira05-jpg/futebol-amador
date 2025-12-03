package com.example.amfootball.ui.viewModel.auth

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.example.amfootball.data.dtos.player.CreateProfileDto
import com.example.amfootball.data.services.AuthService
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.launch
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão da lógica de Autenticação (Login, Registo e Logout).
 *
 * Este ViewModel atua como intermediário entre a UI (Ecrãs de Login/Registo) e a camada de dados ([com.example.amfootball.data.services.AuthService]).
 * Gere o estado de sessão do utilizador e executa operações assíncronas, notificando a UI através de callbacks e StateFlows.
 *
 * @property repository O repositório injetado que contém a lógica de negócio (Firebase + API + Sessão Local).
 */
@HiltViewModel
class AuthViewModel @Inject constructor(
    private val repository: AuthService
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

    /**
     * Função utilitária para alterar o estado de autenticação de forma manual.
     *
     * Usada principalmente no fluxo pós-login, onde o `LoginViewModel` completa o processo
     * e notifica o `AuthViewModel` de que o token foi guardado.
     *
     * @param isUserLoggedIn O novo estado de autenticação.
     */
    fun onIsUserLoggedInChange(isUserLoggedIn: Boolean) {
        _isUserLoggedIn.value = isUserLoggedIn
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
     * Coordena o processo de criação de conta assíncrono.
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

            try {
                repository.registerUser(profile, password)

                _isUserLoggedIn.value = true

                onSuccess()
            } catch (e: Exception) {
                onError(e.message ?: "Erro desconhecido no registo")
            }
        }
    }
}