package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data Class que permite guarda a mensagem de erro a apresentar caso algum filtro da lista de membros seja preenchido incorretamente
 * */
data class FilterMembersFilterError(
    val nameError: ErrorMessage? = null,
    val minAgeError: ErrorMessage? = null,
    val maxAgeError: ErrorMessage? = null
)