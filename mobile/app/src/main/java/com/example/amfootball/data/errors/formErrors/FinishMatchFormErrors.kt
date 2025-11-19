package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Classe que permite validar o formulario de termino de partida
 *
 * */
data class FinishMatchFormErrors(
    val numGoalTeamError: ErrorMessage? = null,
    val numGoalOpponentError: ErrorMessage? = null
)
