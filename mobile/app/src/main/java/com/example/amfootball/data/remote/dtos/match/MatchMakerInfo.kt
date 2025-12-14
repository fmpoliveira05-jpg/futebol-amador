package com.example.amfootball.data.remote.dtos.match

import com.example.amfootball.data.remote.dtos.team.InfoTeamMatchMaker

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
)
