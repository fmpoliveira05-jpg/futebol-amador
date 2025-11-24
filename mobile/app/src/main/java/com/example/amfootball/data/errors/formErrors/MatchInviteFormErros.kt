package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class que guarda todas as mensagens de erro a apresentar caso os campos de data
 * e hora do formulário de convite de partida estejam preenchidos incorretamente.
 *
 * @property dateError Mensagem de erro para o campo de data do jogo.
 * @property timeError Mensagem de erro para o campo de hora do jogo.
 */
data class MatchInviteFormErros(
    val dateError: ErrorMessage? = null,
    val timeError: ErrorMessage? = null
)