package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class que guarda todos os erros do MatchInvite Form
 * */
data class MatchInviteFormErros(
    val dateError: ErrorMessage? = null,
    val timeError: ErrorMessage? = null
)