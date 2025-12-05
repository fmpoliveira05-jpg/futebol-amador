package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class que permite guardar as mensagens de erro a apresentar caso os campos de golos
 * do formulário de finalização de partida sejam preenchidos incorretamente.
 *
 * @property numGoalTeamError Mensagem de erro para o número de golos marcados pela equipa local/própria.
 * @property numGoalOpponentError Mensagem de erro para o número de golos marcados pela equipa adversária.
 */
data class FinishMatchFormErrors(
    val numGoalTeamError: ErrorMessage? = null,
    val numGoalOpponentError: ErrorMessage? = null
)
