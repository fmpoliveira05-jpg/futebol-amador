package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R

/**
 * Enumeração que define a natureza ou tipo de uma partida de futebol.
 *
 * Utilizado para distinguir entre jogos que contam para o ranking (Competitivo) e jogos amigáveis (Casual).
 *
 * @property stringId O ID do recurso de string associado ao tipo de partida para exibição (ex: "Competitivo").
 */
enum class TypeMatch(@StringRes val stringId: Int) {
    /**
     * Indica que a partida é competitiva e os resultados afetam o ranking da equipa.
     */
    COMPETITIVE(stringId = R.string.type_match_competitive),

    /**
     * Indica que a partida é casual (amigável) e os resultados não são registados oficialmente no ranking.
     */
    CASUAL(stringId =  R.string.type_match_casual)
}