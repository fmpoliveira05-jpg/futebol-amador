package com.example.amfootball.data.dtos.match

import com.example.amfootball.data.dtos.support.TeamDto
import java.time.LocalDateTime
import java.util.UUID

/**
 * Data Transfer Object (DTO) que representa os detalhes de uma partida de futebol adiada.
 *
 * É usado para transportar dados sobre o jogo original e sua subsequente remarcação.
 *
 * @property id Identificador único (String, geralmente um UUID) da partida adiada.
 * @property opponent O objeto [TeamDto] que representa a equipa adversária.
 * @property gameDate A data e hora originais em que a partida deveria ter ocorrido.
 * @property postPoneDate A nova data e hora em que a partida foi remarcada após o adiamento.
 * @property pitchMatch O nome do campo ou estádio onde a partida será realizada.
 */
data class PostPoneMatchDto(
    val id: String,
    val opponent: TeamDto,
    val gameDate: LocalDateTime,
    val postPoneDate: LocalDateTime,
    val pitchMatch: String
) {
    companion object {
        fun createExamplePostPoneMatchList(): List<PostPoneMatchDto> {
            val teamNames = listOf(
                "Águias de Lisboa", "Dragões do Porto", "Leões de Alvalade",
                "Guerreiros de Braga", "Panteras da Boavista", "Canarinhos de Viseu",
                "Lobos de Famalicão", "Castores de Paços", "Gilistas de Barcelos"
            )
            val pitchNames = listOf(
                "Estádio Municipal", "Campo Principal", "Centro de Treinos GSC",
                "Complexo Desportivo", "Campo da Relva", "Estádio do Bairro"
            )

            // --- A ÚNICA MUDANÇA É AQUI ---
            // O Preview não consegue correr LocalDateTime.now() no init do ViewModel.
            // Por isso, damos-lhe uma data fixa que ele consegue processar.
            val now = LocalDateTime.of(2025, 11, 15, 17, 30) // Ex: 15/11/2025 17:30

            // Gera a lista de 20 itens
            return (1..20).map { index ->

                val gameDate = now.plusDays(index.toLong())
                    .withHour((10..20).random())
                    .withMinute(listOf(0, 15, 30, 45).random())

                val postPoneDate = gameDate.plusDays((7..30).random().toLong())

                val opponent = TeamDto(
                    id = UUID.randomUUID().toString(),
                    name = teamNames.random()
                )

                PostPoneMatchDto(
                    id = UUID.randomUUID().toString(),
                    opponent = opponent,
                    gameDate = gameDate,
                    postPoneDate = postPoneDate,
                    pitchMatch = pitchNames.random()
                )
            }
        }
    }
}