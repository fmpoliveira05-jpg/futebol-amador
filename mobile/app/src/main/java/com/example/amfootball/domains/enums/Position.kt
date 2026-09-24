package com.example.amfootball.domains.enums

import androidx.annotation.StringRes
import com.example.amfootball.R
import com.google.gson.annotations.SerializedName

/**
 * Enumeração que define as posições principais de um jogador de futebol em campo.
 *
 * A ordem (e por isso o `ordinal`, que é o valor enviado à API) tem de ser a mesma do enum
 * `Position` do backend: FORWARD = 0, MIDFIELDER = 1, DEFENDER = 2, GOALKEEPER = 3.
 * Antes estava invertida, e um avançado ficava registado como guarda-redes.
 *
 * Utiliza [SerializedName] para aceitar o número (como String) ou o nome em inglês no JSON.
 *
 * @property stringId O ID do recurso de string associado à posição para exibição na UI (ex: "Guarda-Redes").
 */
enum class Position(@StringRes val stringId: Int) {
    /** O Avançado. Aceita "0" ou "Forward". */
    @SerializedName("0", alternate = ["Forward"])
    FORWARD(stringId = R.string.position_forward),

    /** O Médio. Aceita "1" ou "Midfielder". */
    @SerializedName("1", alternate = ["Midfielder"])
    MIDFIELDER(stringId = R.string.position_midfields),

    /** O Defesa. Aceita "2" ou "Defender". */
    @SerializedName("2", alternate = ["Defender"])
    DEFENDER(stringId = R.string.position_defender),

    /** O Guarda-Redes. Aceita "3", "Goalkeeper" ou "GoalKeeper". */
    @SerializedName("3", alternate = ["GoalKeeper", "Goalkeeper"])
    GOALKEEPER(stringId = R.string.position_goalkeeper),
}
