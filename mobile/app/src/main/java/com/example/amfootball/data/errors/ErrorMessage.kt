package com.example.amfootball.data.errors

/**
 * Data class que representa uma mensagem de erro que deve ser exibida ao utilizador.
 *
 * Esta estrutura permite usar strings de recurso parametrizadas (strings.xml)
 * para suportar diferentes idiomas e injetar valores dinâmicos.
 *
 * @property messageId O ID do recurso de string (R.string) que contém o texto da mensagem de erro.
 * @property args Argumentos opcionais a serem formatados na string de recurso (ex: limites de validação).
 */
data class ErrorMessage(
    val messageId: Int,
    val args: List<Any> = emptyList()
)
