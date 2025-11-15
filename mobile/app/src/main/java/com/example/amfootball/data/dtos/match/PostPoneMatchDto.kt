package com.example.amfootball.data.dtos.match

import com.example.amfootball.data.dtos.OpponentDto
import java.time.LocalDateTime
import java.util.UUID

data class PostPoneMatchDto(
    val id: String,
    val opponent: OpponentDto,
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

                val opponent = OpponentDto(
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