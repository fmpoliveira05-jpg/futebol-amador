package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class que guarda as mensagens de erro a apresentar quando algum filtro da lista de jogadores é preenchido incorretamente.
 *
 * Utilizado para reportar erros específicos de validação dos campos de filtro de forma concisa.
 *
 * @property nameError Mensagem de erro para o filtro de nome.
 * @property cityError Mensagem de erro para o filtro de cidade.
 * @property minAgeError Mensagem de erro para o filtro de idade mínima.
 * @property maxAgeError Mensagem de erro para o filtro de idade máxima.
 * @property minSizeError Mensagem de erro para o filtro de altura mínima (em centímetros).
 * @property maxSizeError Mensagem de erro para o filtro de altura máxima (em centímetros).
 */
data class FilterPlayersErrors(
    val nameError: ErrorMessage? = null,
    val cityError: ErrorMessage? = null,
    val minAgeError: ErrorMessage? = null,
    val maxAgeError: ErrorMessage? = null,
    val minSizeError: ErrorMessage? = null,
    val maxSizeError: ErrorMessage? = null,
)