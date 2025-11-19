package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
* Classe que guarda as mensagens de erro de cada um dos filtros da lista de pedidos de partida
* */
data class FilterMatchInviteError(
    val senderNameError: ErrorMessage? = null,
    val minDateError: ErrorMessage? = null,
    val maxDateError: ErrorMessage? = null,
)