package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data classe que guarda as mensagem de erro a apresentar quando algum filtro do calendar está mal preenchido
 * */
data class FilterCalendarError(
    val opponentNameError: ErrorMessage? = null,
    val minGameDateError: ErrorMessage? = null,
    val maxGameDateError: ErrorMessage? = null,
)
