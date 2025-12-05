package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class que guarda as mensagem de erro a apresentar quando algum filtro do calendário está mal preenchido.
 *
 * Utilizado para reportar erros específicos de validação de filtros de forma concisa.
 *
 * @property opponentNameError Mensagem de erro para o filtro de nome do adversário (null se for válido).
 * @property minGameDateError Mensagem de erro para o filtro de data mínima de jogo.
 * @property maxGameDateError Mensagem de erro para o filtro de data máxima de jogo.
 */
data class FilterCalendarError(
    val opponentNameError: ErrorMessage? = null,
    val minGameDateError: ErrorMessage? = null,
    val maxGameDateError: ErrorMessage? = null,
)
