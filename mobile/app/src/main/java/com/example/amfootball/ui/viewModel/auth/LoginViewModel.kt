package com.example.amfootball.ui.viewModel.auth

import com.example.amfootball.data.dtos.player.LoginDto
import com.example.amfootball.data.errors.ErrorMessage
import com.example.amfootball.data.network.NetworkConnectivityObserver
import com.example.amfootball.data.services.AuthService
import com.example.amfootball.ui.viewModel.abstracts.BaseViewModel
import com.example.amfootball.R
import com.example.amfootball.data.dtos.team.FormTeamDto
import com.example.amfootball.data.errors.formErrors.LoginError
import com.example.amfootball.data.errors.formErrors.TeamFormErros
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.viewModel.abstracts.FormsViewModel
import com.example.amfootball.utils.UserConst
import dagger.hilt.android.lifecycle.HiltViewModel
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import javax.inject.Inject

/**
 * ViewModel responsável pela gestão da lógica de negócio e estado do ecrã de Login.
 *
 * Esta classe gere os dados de entrada do utilizador (Email e Password) e coordena a comunicação
 * com a camada de dados ([AuthService]) para efetuar a autenticação.
 *
 * Herda de [BaseViewModel] para tirar partido das funcionalidades de gestão de conectividade
 * ([NetworkConnectivityObserver]) e tratamento de erros/loading através do [launchDataLoad].
 *
 * @property repository Serviço responsável pelas chamadas à API de autenticação.
 * @property networkObserver Observador de conectividade de rede (injetado no BaseViewModel).
 */
@HiltViewModel
class LoginViewModel @Inject constructor(
    private val repository: AuthService,
    private val networkObserver: NetworkConnectivityObserver
) : FormsViewModel<LoginDto, LoginError>(
    networkObserver = networkObserver,
    initialData = LoginDto(),
    initialError = LoginError()
) {
    init {
        stopLoading()
    }

    /**
     * Atualiza o campo de email no estado atual.
     *
     * Utiliza o método `.copy()` do Data Class para garantir a imutabilidade dos restantes campos,
     * atualizando apenas o email. Dispara uma nova emissão para o [uiLoginState].
     *
     * @param newEmail O novo texto de email inserido pelo utilizador.
     */
    fun onEmailChange(newEmail: String) {
        formState.value = formState.value.copy(email = newEmail)
    }

    /**
     * Atualiza o campo de password no estado atual.
     *
     * Utiliza o método `.copy()` do Data Class para garantir a imutabilidade dos restantes campos,
     * atualizando apenas a password. Dispara uma nova emissão para o [uiLoginState].
     *
     * @param newPassword O novo texto da password inserido pelo utilizador.
     */
    fun onPasswordChange(newPassword: String) {
        formState.value = formState.value.copy(password = newPassword)
    }

    /**
     * Executa o processo de login de forma assíncrona e segura.
     *
     * O fluxo de execução é o seguinte:
     * 1. Verifica a conectividade com a internet através da função [onlineFunctionality].
     * Se não houver internet, exibe uma mensagem Toast e cancela a operação.
     * 2. Inicia uma corrotina gerida pelo [launchDataLoad] (que gere estados de Loading e Erro).
     * 3. Chama o repositório para autenticar o utilizador.
     * 4. Retorna o resultado através do callback [onResult].
     *
     * @param login O objeto DTO contendo as credenciais (Email e Password) a enviar.
     * @param onResult Callback executado quando a operação termina.
     * Recebe `true` se a autenticação for bem-sucedida, `false` caso contrário.
     */
    fun loginUser(login: LoginDto, onResult: (Boolean) -> Unit) {
        submitForm(
            apiCall = {
                val success = repository.loginUser(login)
                onResult(success)
            }
        )
    }

    override fun validateForm(): Boolean {
        val email = formState.value.email
        val emailLength = email.length
        val password = formState.value.password
        val passwordLength = password.length

        var emailError: ErrorMessage? = null
        var passwordError: ErrorMessage? = null

        if (email.isBlank()) {
            emailError = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        } else {
            if (emailLength < UserConst.MIN_EMAIL_LENGTH) {
                emailError = ErrorMessage(
                    messageId = R.string.error_min_email_length,
                    args = listOf(UserConst.MIN_EMAIL_LENGTH)
                )
            } else if(emailLength > UserConst.MAX_EMAIL_LENGTH) {
                emailError = ErrorMessage(
                    messageId = R.string.error_max_email_length,
                    args = listOf(UserConst.MAX_EMAIL_LENGTH)
                )
            }
        }

        if (password.isBlank()) {
            passwordError = ErrorMessage(
                messageId = R.string.mandatory_field
            )
        } else {
            if (passwordLength < UserConst.MIN_PASSWORD_LENGTH) {
                passwordError = ErrorMessage(
                    messageId = R.string.error_min_password_length,
                    args = listOf(UserConst.MIN_PASSWORD_LENGTH)
                )
            } else if (passwordLength > UserConst.MAX_PASSWORD_LENGTH) {
                passwordError = ErrorMessage(
                    messageId = R.string.error_max_password_length,
                    args = listOf(UserConst.MAX_PASSWORD_LENGTH)
                )
            }
        }

        formErrors.value = LoginError(
            emailErrorMessage = emailError,
            passwordError = passwordError
        )

        val isValid = listOf(emailError, passwordError).all { it == null }

        return isValid
    }
}