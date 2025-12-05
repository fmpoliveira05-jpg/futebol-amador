package com.example.amfootball.data.dtos

/**
 * Data Transfer Object (DTO) que representa o modelo de dados de um Formulário de Campo de Futebol (Pitch).
 *
 * É utilizado para transportar informações de criação ou edição de um campo de jogo.
 *
 * @property id O identificador único do campo. É nullable, indicando que o campo pode estar em processo de criação.
 * @property name O nome do campo ou estádio.
 * @property address O endereço físico completo do campo.
 */
data class PitchFormDto(
    val id: String? = null,
    val name: String = "",
    val address: String = ""
)