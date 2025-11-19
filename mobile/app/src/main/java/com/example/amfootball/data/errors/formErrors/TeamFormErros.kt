package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data Class que guarda todos os erros dos campos do Team Form
 * */
data class TeamFormErros(
    val nameError: ErrorMessage? = null,
    val descriptionError: ErrorMessage? = null,
    val pitchNameError: ErrorMessage? = null,
    val pitchAddressError: ErrorMessage? = null,
)