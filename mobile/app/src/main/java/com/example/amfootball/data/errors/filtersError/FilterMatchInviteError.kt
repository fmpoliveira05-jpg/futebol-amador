package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class que guarda as mensagens de erro a apresentar quando algum filtro da lista de pedidos de partida está mal preenchido.
 *
 * Utilizado para reportar erros específicos de validação de filtros de forma concisa.
 *
 * @property senderNameError Mensagem de erro para o filtro de nome do remetente (quem enviou o convite).
 * @property minDateError Mensagem de erro para o filtro de data mínima de envio/receção do convite.
 * @property maxDateError Mensagem de erro para o filtro de data máxima de envio/receção do convite.
 */
data class FilterMatchInviteError(
    val senderNameError: ErrorMessage? = null,
    val minDateError: ErrorMessage? = null,
    val maxDateError: ErrorMessage? = null,
)