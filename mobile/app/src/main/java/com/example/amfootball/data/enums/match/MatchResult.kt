package com.example.amfootball.data.enums.match

import androidx.annotation.StringRes
import com.example.amfootball.R

/**
 * Enumeração que define o resultado de uma partida no contexto de uma equipa.
 *
 * Cada valor está associado a um ID de recurso de string ([stringId]) para permitir
 * a tradução e exibição do resultado na interface do utilizador.
 *
 * @property stringId O ID do recurso de string associado ao resultado para exibição (ex: "Vitória").
 */
enum class MatchResult(@StringRes val stringId: Int) {
    /**
     * Indica que a equipa obteve uma vitória na partida.
     */
    WIN(stringId = R.string.match_result_win),

    /**
     * Indica que a partida resultou num empate para a equipa.
     */
    DRAW(stringId = R.string.match_result_draw),

    /**
     * Indica que a equipa sofreu uma derrota na partida.
     */
    LOSE(stringId = R.string.match_result_lose),

    /**
     * Indica que o resultado da partida ainda não está definido,
     * ou seja, o jogo ainda não terminou ou não foi jogado.
     */
    UNDEFINED(stringId = R.string.match_result_undefined),
}