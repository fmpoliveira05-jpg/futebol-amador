package com.example.amfootball.data.actions.forms

import com.example.amfootball.data.dtos.player.LoginDto

/**
 * Encapsula todas as interações e eventos de UI possíveis no ecrã de Login.
 *
 * Esta classe é utilizada para implementar o padrão de **State Hoisting**, permitindo que
 * os componentes de UI (Composables ou Views) sejam "stateless" e deleguem a lógica
 * das ações para um controlador (geralmente um ViewModel).
 *
 * @property onLoginUser Ação para submeter o formulário de login.*
 * @property onEmailChange Ação disparada sempre que o conteúdo do campo de texto do Email é alterado.
 * @property onPasswordChange Ação disparada sempre que o conteúdo do campo de texto da Password é alterado.
 * @property onIsUserLoggedInChange Ação utilizada para atualizar explicitamente o estado de sessão do utilizador
 */
data class LoginActions(
    val onLoginUser: (login: LoginDto, onResult: (Boolean) -> Unit) -> Unit,
    val onEmailChange: (String) -> Unit,
    val onPasswordChange: (String) -> Unit,
    val onIsUserLoggedInChange: (Boolean) -> Unit
)
