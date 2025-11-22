package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data Class que guarda todas as mensagens de erro a apresentar caso os campos
 * do Formulário de Equipa estejam preenchidos incorretamente.
 *
 * @property nameError Mensagem de erro para o campo de nome da equipa.
 * @property descriptionError Mensagem de erro para o campo de descrição da equipa.
 * @property pitchNameError Mensagem de erro para o campo de nome do campo de jogo.
 * @property pitchAddressError Mensagem de erro para o campo de endereço do campo de jogo.
 */
data class TeamFormErros(
    val nameError: ErrorMessage? = null,
    val descriptionError: ErrorMessage? = null,
    val pitchNameError: ErrorMessage? = null,
    val pitchAddressError: ErrorMessage? = null,
)