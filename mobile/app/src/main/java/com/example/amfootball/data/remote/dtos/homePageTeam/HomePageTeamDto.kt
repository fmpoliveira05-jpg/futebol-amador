package com.example.amfootball.data.remote.dtos.homePageTeam

import com.example.amfootball.data.remote.dtos.match.InfoMatch
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.google.gson.annotations.SerializedName

/**
 * Data Transfer Object (DTO) que representa todos os dados necessários para popular
 * o ecrã principal da equipa (Team Home Page/Dashboard).
 *
 * Este DTO é o resultado da chamada à API que agrega a informação básica da equipa,
 * os próximos jogos agendados e o histórico de forma recente.
 *
 * @property team O DTO [TeamDto] que contém os dados de identificação e informações base da equipa.
 * O valor predefinido é um [TeamDto] vazio, garantindo a não-nulidade.
 *
 * @property nextsMatch A lista dos próximos jogos agendados para a equipa.
 * - Utiliza a anotação [@SerializedName] para mapear corretamente tanto "NextsMatchs" (CamelCase)
 * quanto "nextsMatchs" (lowerCamelCase) vindos da API.
 * - O tipo de cada item na lista é [InfoMatch].
 * - O valor predefinido é uma lista vazia, garantindo a não-nulidade.
 *
 * @property vitorySequenceTeam A lista do histórico de resultados dos jogos anteriores (forma recente).
 * - Utiliza a anotação [@SerializedName] para mapear corretamente tanto "HistoricPreviousGames"
 * quanto "historicPreviousGames" vindos da API.
 * - O tipo de cada item na lista é [VitorySequenceTemDto].
 * - O valor predefinido é uma lista vazia, garantindo a não-nulidade.
 */
data class HomePageTeamDto(
    val team: TeamDto = TeamDto(),
    @SerializedName("NextsMatchs", alternate = ["nextsMatchs"])
    val nextsMatch: List<InfoMatch> = emptyList(),
    @SerializedName("HistoricPreviousGames", alternate = ["historicPreviousGames"])
    val vitorySequenceTeam: List<VitorySequenceTemDto> = emptyList()
)
