package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

data class LoginError(
    val emailErrorMessage: ErrorMessage? = null,
    val passwordError: ErrorMessage? = null,
)
