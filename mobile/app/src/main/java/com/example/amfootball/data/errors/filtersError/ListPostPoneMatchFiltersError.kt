package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class que guarda as mensagens de erro a apresentar quando algum filtro da lista de partidas adiadas é preenchido incorretamente.
 *
 * Inclui validações para as datas originais do jogo e para as datas de remarcação.
 *
 * @property nameOpponentError Mensagem de erro para o filtro de nome do adversário.
 * @property minDateGameError Mensagem de erro para a data mínima original do jogo.
 * @property maxDateGameError Mensagem de erro para a data máxima original do jogo.
 * @property minDatePostPoneError Mensagem de erro para a data mínima de adiamento/remarcação.
 * @property maxDatePostPoneError Mensagem de erro para a data máxima de adiamento/remarcação.
 */
data class ListPostPoneMatchFiltersError(
    val nameOpponentError: ErrorMessage? = null,
    val minDateGameError: ErrorMessage? = null,
    val maxDateGameError: ErrorMessage? = null,
    val minDatePostPoneError: ErrorMessage? = null,
    val maxDatePostPoneError: ErrorMessage? = null
)