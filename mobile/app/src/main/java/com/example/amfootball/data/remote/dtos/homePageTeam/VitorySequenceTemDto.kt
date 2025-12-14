package com.example.amfootball.data.remote.dtos.homePageTeam

import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.domains.enums.match.MatchResult
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa o resultado de uma única partida histórica
 * para fins de cálculo ou exibição da "Forma Recente" da equipa.
 *
 * Este objeto é usado para alimentar o componente visual que mostra W, L ou D.
 *
 * @property opponent O DTO [TeamDto] do adversário contra o qual o jogo foi disputado.
 * @property result A string literal que representa o resultado (ex: "W", "L", "D" ou "2-1").
 * Tipicamente é o valor textual a ser exibido na insígnia.
 * @property matchResultId O ID numérico do resultado da partida conforme definido na API/Backend.
 * Este ID é usado para mapear o resultado para o enum [MatchResult].
 */
data class VitorySequenceTemDto(
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamDto = TeamDto(),
    @SerializedName("Result", alternate = ["result"])
    val result: String = "",
    @SerializedName("MatchResult", alternate = ["matchResult"])
    val matchResultId: Int = 0
) {
    /**
     * Propriedade calculada (Computed Property) que traduz o [matchResultId] numérico
     * para o enum [MatchResult] tipado, garantindo a segurança de tipos no frontend.
     *
     * Mapeamento:
     * - 0: [MatchResult.WIN] (Vitória)
     * - 1: [MatchResult.LOSE] (Derrota)
     * - 2: [MatchResult.DRAW] (Empate)
     * - Outro: [MatchResult.UNDEFINED] (Resultado Desconhecido/Inválido)
     *
     * @return O enum [MatchResult] correspondente ao ID do resultado da partida.
     */
    val matchResult: MatchResult
        get() = when (matchResultId) {
            0 -> MatchResult.WIN
            1 -> MatchResult.LOSE
            2 -> MatchResult.DRAW
            else -> MatchResult.UNDEFINED
        }
}