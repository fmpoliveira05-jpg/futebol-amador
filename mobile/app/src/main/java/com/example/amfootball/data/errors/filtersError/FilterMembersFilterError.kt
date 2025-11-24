package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data Class que guarda as mensagem de erro a apresentar caso algum filtro da lista de membros seja preenchido incorretamente.
 *
 * Utilizado para reportar erros específicos de validação dos campos de filtro de forma concisa.
 *
 * @property nameError Mensagem de erro para o filtro de nome.
 * @property minAgeError Mensagem de erro para o filtro de idade mínima.
 * @property maxAgeError Mensagem de erro para o filtro de idade máxima.
 */
data class FilterMembersFilterError(
    val nameError: ErrorMessage? = null,
    val minAgeError: ErrorMessage? = null,
    val maxAgeError: ErrorMessage? = null
)