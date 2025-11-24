package com.example.amfootball.data.dtos.match

import com.example.amfootball.data.dtos.team.InfoTeamMatchMaker
import java.util.UUID

/**
 * Data Transfer Object (DTO) que agrupa as informações necessárias para o processo de Matchmaking.
 *
 * Este objeto é usado para exibir ou transportar dados sobre as equipas que estão à procura de jogo
 * ou que já estão emparelhadas, juntamente com o local do jogo.
 *
 * @property team Lista de equipas ([InfoTeamMatchMaker]) envolvidas no processo. Pode conter uma (procura) ou duas (jogo definido).
 * @property pitch O nome do campo ou estádio onde a partida será realizada (padrão: string vazia).
 */
data class MatchMakerInfo(
    val team: List<InfoTeamMatchMaker>,
    val pitch: String = ""
) {
    companion object {
        fun createExampleWithOneTeam(): MatchMakerInfo {
            val UnicaEquipa = InfoTeamMatchMaker(
                id = UUID.randomUUID().toString(),
                name = "Equipa Solitária",
                rank = "Ouro"
            )

            return MatchMakerInfo(
                team = listOf(UnicaEquipa)
            )
        }
    }
}
