package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

data class SignUpFormErrors(
    val nameError: ErrorMessage? = null,
    val emailError: ErrorMessage? = null,
    val phoneError: ErrorMessage? = null,
    val addressError: ErrorMessage? = null,
    val heightError: ErrorMessage? = null,
    val dateError: ErrorMessage? = null,
    val positionError: ErrorMessage? = null,
    val passwordError: ErrorMessage? = null,
    val passwordVerifyError: ErrorMessage? = null
)