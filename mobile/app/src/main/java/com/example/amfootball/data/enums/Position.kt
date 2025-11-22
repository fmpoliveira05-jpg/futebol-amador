package com.example.amfootball.data.enums

import androidx.annotation.StringRes
import com.example.amfootball.R
import com.google.gson.annotations.SerializedName

/**
 * Enumeração que define as posições principais de um jogador de futebol em campo.
 *
 * Utiliza [SerializedName] para permitir a desserialização flexível a partir de diferentes
 * formatos de entrada JSON (números como String ou nomes em inglês).
 *
 * @property stringId O ID do recurso de string associado à posição para exibição na UI (ex: "Guarda-Redes").
 */
enum class Position(@StringRes val stringId: Int) {
    /**
     * O Guarda-Redes. Aceita os valores "0", "Goalkeeper" ou "GoalKeeper" na desserialização JSON.
     */
    @SerializedName("0", alternate = ["GoalKeeper", "Goalkeeper"]) // Aceita nº 0 ou String
    GOALKEEPER(stringId = R.string.position_goalkeeper),

    /**
     * O Defesa. Aceita os valores "1" ou "Defender" na desserialização JSON.
     */
    @SerializedName("1", alternate = ["Defender"])
    DEFENDER(stringId = R.string.position_defender),

    /**
     * O Médio. Aceita os valores "2" ou "Midfielder" na desserialização JSON.
     */
    @SerializedName("2", alternate = ["Midfielder"])
    MIDFIELDER(stringId = R.string.position_midfields),

    /**
     * O Avançado. Aceita os valores "3" ou "Forward" na desserialização JSON.
     */
    @SerializedName("3", alternate = ["Forward"])
    FORWARD(stringId = R.string.position_forward),
}