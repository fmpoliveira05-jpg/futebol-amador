package com.example.amfootball.data.errors.filtersError

import com.example.amfootball.data.errors.ErrorMessage

/**
 * Data class responsável por armazenar os estados de erro da validação dos filtros de equipas.
 *
 * Cada propriedade representa um erro específico associado a um campo do formulário de filtragem.
 * Se uma propriedade for `null`, significa que o campo correspondente é válido.
 * Utilizada na UI para apresentar feedback visual (mensagens de erro) ao utilizador nos campos de input.
 *
 * @property nameError Contém o erro associado ao campo de nome (ex: limite de caracteres excedido).
 * @property cityError Contém o erro associado ao campo de cidade.
 * @property minPointError Contém o erro associado ao valor mínimo de pontos (ex: valor negativo ou superior ao máximo).
 * @property maxPointError Contém o erro associado ao valor máximo de pontos.
 * @property minAgeError Contém o erro associado à idade mínima (ex: intervalo inválido).
 * @property maxAgeError Contém o erro associado à idade máxima.
 * @property minNumberMembersError Contém o erro associado ao nº mínimo de membros.
 * @property maxNumberMembersError Contém o erro associado ao nº máximo de membros.
 */
data class FilterTeamError(
    val nameError: ErrorMessage? = null,
    val cityError: ErrorMessage? = null,
    val minPointError: ErrorMessage? = null,
    val maxPointError: ErrorMessage? = null,
    val minAgeError: ErrorMessage? = null,
    val maxAgeError: ErrorMessage? = null,
    val minNumberMembersError: ErrorMessage? = null,
    val maxNumberMembersError: ErrorMessage? = null
)