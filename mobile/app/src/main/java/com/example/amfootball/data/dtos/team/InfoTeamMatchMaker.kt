package com.example.amfootball.data.dtos.team

import android.net.Uri

/**
 * Data Transfer Object (DTO) que representa a informação essencial de uma Equipa
 * necessária para o processo de Matchmaking (procura de adversário).
 *
 * É leve e focado nas propriedades relevantes para o ranking e identificação.
 *
 * @property id O identificador único da equipa.
 * @property name O nome da equipa.
 * @property logoTeam A URI do logótipo da equipa. Padrão é [Uri.EMPTY].
 * @property rank O nível ou nome do rank atual da equipa, usado para emparelhamento justo.
 */
data class InfoTeamMatchMaker(
    val id: String,
    val name: String,
    val logoTeam: String? = null,
    val rank: String
)
