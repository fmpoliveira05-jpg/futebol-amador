package com.example.amfootball.domains.enums

/**
 * Define os possíveis estados de resposta a um pedido de adiamento de jogo (Postpone Match).
 *
 * Cada estado possui um valor inteiro associado (`value`) que é usado para comunicação
 * com a API de backend.
 *
 * @property value O valor inteiro que representa o estado na camada de dados (API).
 */
enum class StatusPostPone(val value: Int) {
    /**
     * O pedido de adiamento foi **ACEITE** pela equipa adversária.
     * Corresponde ao valor inteiro 0 na API.
     */
    ACCEPTED(0),

    /**
     * O pedido de adiamento foi **REJEITADO** pela equipa adversária.
     * Corresponde ao valor inteiro 1 na API.
     */
    REJECTED(1)
}