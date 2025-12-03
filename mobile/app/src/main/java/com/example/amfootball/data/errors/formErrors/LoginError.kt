package com.example.amfootball.data.errors.formErrors

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Agrega os estados de erro de validação para os campos do formulário de Login.
 *
 * Esta classe é utilizada dentro do estado da UI (UI State) para indicar visualmente
 * se existem problemas com os dados inseridos pelo utilizador.
 *
 * Se uma propriedade for `null`, assume-se que o campo correspondente passou na validação.
 *
 * @property emailErrorMessage O objeto [ErrorMessage] associado ao campo de **Email**.
 * Conterá a descrição do erro (ex: "Formato inválido" ou "Campo obrigatório") caso a validação falhe.
 *
 * @property passwordError O objeto [ErrorMessage] associado ao campo de **Password**.
 * Conterá a descrição do erro (ex: "A palavra-passe deve ter no mínimo 6 caracteres") caso a validação falhe.
 */
data class LoginError(
    val emailErrorMessage: ErrorMessage? = null,
    val passwordError: ErrorMessage? = null,
)
