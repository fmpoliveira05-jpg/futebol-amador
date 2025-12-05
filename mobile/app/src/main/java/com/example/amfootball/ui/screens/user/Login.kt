package com.example.amfootball.ui.screens.user

import android.net.Uri
import android.util.Log
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.testTag
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.UiState
import com.example.amfootball.data.actions.forms.LoginActions
import com.example.amfootball.data.dtos.player.LoginDto
import com.example.amfootball.data.errors.formErrors.LoginError
import com.example.amfootball.navigation.objects.Routes
import com.example.amfootball.ui.components.LoadingPage
import com.example.amfootball.ui.components.buttons.LoginButton
import com.example.amfootball.ui.components.inputFields.EmailTextField
import com.example.amfootball.ui.components.inputFields.PasswordTextField
import com.example.amfootball.ui.components.notification.ToastHandler
import com.example.amfootball.ui.viewModel.auth.AuthViewModel
import com.example.amfootball.ui.viewModel.auth.LoginViewModel

/**
 * Ecrã de Login (Autenticação).
 *
 * Este componente "Stateful" (com estado) atua como o controlador para o processo de autenticação.
 *
 * Responsabilidades:
 * 1. Observar o estado do formulário, erros e UI (Loading/Erro).
 * 2. Orquestrar a submissão do login via [LoginViewModel].
 * 3. Gerir o complexo fluxo de navegação pós-login (incluindo o redirecionamento de rotas protegidas).
 *
 * @param navHostController Controlador de navegação.
 * @param viewModel ViewModel que gere a lógica do formulário e chamada à API de Login.
 * @param authViewModel ViewModel que gere o estado global de autenticação (usado para atualizar o token na app).
 */
@Composable
fun LoginScreen(
    navHostController: NavHostController,
    viewModel: LoginViewModel = hiltViewModel(),
    authViewModel: AuthViewModel = hiltViewModel()
) {
    val login by viewModel.uiFormState.collectAsStateWithLifecycle()
    val errors by viewModel.uiFormErrors.collectAsStateWithLifecycle()
    val uiState by viewModel.uiState.collectAsStateWithLifecycle()

    ToastHandler(
        toastMessage = uiState.toastMessage,
        onToastShown = viewModel::onToastShown
    )

    ContentLogin(
        navHostController = navHostController,
        login = login,
        errors = errors,
        uiState = uiState,
        loginActions = LoginActions(
            onLoginUser = viewModel::loginUser,
            onEmailChange = viewModel::onEmailChange,
            onPasswordChange = viewModel::onPasswordChange,
            onIsUserLoggedInChange = authViewModel::onIsUserLoggedInChange,
        ),
        modifier = Modifier
            .padding(16.dp),
    )
}

/**
 * Conteúdo visual do ecrã de Login (Stateless).
 *
 * Responsável por estruturar a página e envolver o formulário no componente de [LoadingPage].
 *
 * @param uiState Estado da UI (Loading, Erros de conexão).
 * @param login DTO com os valores atuais do formulário (email/password).
 * @param errors DTO com as mensagens de erro de validação específicas de cada campo.
 * @param loginActions Callbacks para interagir com o formulário e submeter.
 * @param navHostController Controlador de navegação.
 * @param modifier Modificador de layout.
 */
@Composable
private fun ContentLogin(
    uiState: UiState,
    login: LoginDto,
    errors: LoginError,
    loginActions: LoginActions,
    navHostController: NavHostController,
    modifier: Modifier = Modifier
) {
    LoadingPage(
        isLoading = uiState.isLoading,
        errorMsg = uiState.errorMessage,
        retry = {},
        content = {
            Column(
                modifier = modifier,
                verticalArrangement = Arrangement.Center,
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                FieldsLogin(
                    formLogin = login,
                    errors = errors,
                    loginActions = loginActions,
                    navHostController = navHostController
                )
            }
        }
    )
}

/**
 * Campos de input e Botão de Submissão do Login.
 *
 * Inclui a lógica de navegação complexa pós-login.
 *
 * @param formLogin Dados atuais.
 * @param loginActions Ações para submissão e alteração de valores.
 * @param errors Erros de validação.
 * @param navHostController Controlador de navegação.
 */
@Composable
private fun FieldsLogin(
    formLogin: LoginDto,
    loginActions: LoginActions,
    errors: LoginError,
    navHostController: NavHostController
) {
    Column(
        modifier = Modifier.fillMaxWidth(),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(16.dp)
    ) {
        Text(text = stringResource(id = R.string.title_login))

        Column(
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            EmailTextField(
                value = formLogin.email,
                isError = errors.emailErrorMessage != null,
                errorMessage = errors.emailErrorMessage?.let {
                    stringResource(id = it.messageId, *it.args.toTypedArray())
                },
                onValueChange = { loginActions.onEmailChange(it) },
                textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_email_input))
            )

            PasswordTextField(
                label = stringResource(id = R.string.password_label),
                value = formLogin.password,
                isError = errors.passwordError != null,
                errorMessage = errors.passwordError?.let {
                    stringResource(id = it.messageId, *it.args.toTypedArray())
                },
                onValueChange = { loginActions.onPasswordChange(it) },
                textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_password_input))
            )
        }

        LoginButton(
            onClick = {
                loginActions.onLoginUser(
                    formLogin,
                    { loginComSucesso ->
                        Log.d("TESTE_UI", "Resultado do Login: $loginComSucesso")
                        if (loginComSucesso) {
                            loginActions.onIsUserLoggedInChange(true)
                            val redirectRoute = navHostController.currentBackStackEntry
                                ?.arguments
                                ?.getString("redirect")

                            if (redirectRoute != null) {
                                val decodedRoute = Uri.decode(redirectRoute)
                                navHostController.navigate(decodedRoute) {
                                    popUpTo("${Routes.UserRoutes.LOGIN}?redirect={redirect}") {
                                        inclusive = true
                                    }
                                }
                            } else {
                                navHostController.navigate(Routes.GeralRoutes.HOMEPAGE.route) {
                                    popUpTo(navHostController.graph.startDestinationId) {
                                        inclusive = true
                                    }
                                }
                            }
                        }
                    }
                )
            },
            textFieldModifier = Modifier.testTag(stringResource(id = R.string.tag_login_button)),
        )
    }
}

@Preview(name = "Login Screen - EN", locale = "en", showBackground = true)
@Preview(name = "Login Screen - PT", locale = "pt", showBackground = true)
@Composable
fun LoginScreenPreview() {
    ContentLogin(
        uiState = UiState(isLoading = false),
        login = LoginDto(email = "test@example.com", password = ""),
        errors = LoginError(),
        loginActions = LoginActions(
            onLoginUser = { _, onResult ->
                onResult(true)
            },
            onEmailChange = {},
            onPasswordChange = {},
            onIsUserLoggedInChange = {}
        ),
        navHostController = rememberNavController()
    )
}

@Preview(name = "Login Loading State", showBackground = true)
@Composable
fun LoginScreenLoadingPreview() {
    ContentLogin(
        uiState = UiState(isLoading = true),
        login = LoginDto(),
        errors = LoginError(),
        loginActions = LoginActions(
            onLoginUser = { _, onResult ->
                onResult(false)
            },
            onEmailChange = {},
            onPasswordChange = {},
            onIsUserLoggedInChange = {}
        ),
        navHostController = rememberNavController()
    )
}