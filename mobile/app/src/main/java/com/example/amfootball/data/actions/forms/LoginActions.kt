package com.example.amfootball.data.actions.forms

import com.example.amfootball.data.dtos.player.LoginDto

data class LoginActions(
    val onLoginUser: (login: LoginDto, onResult: (Boolean) -> Unit) -> Unit,
    val onEmailChange: (String) -> Unit,
    val onPasswordChange: (String) -> Unit,
    val onIsUserLoggedInChange: (Boolean) -> Unit
)
