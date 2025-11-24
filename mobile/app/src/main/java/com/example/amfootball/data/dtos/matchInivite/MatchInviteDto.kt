package com.example.amfootball.data.dtos.matchInivite

import com.example.amfootball.utils.Patterns
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

/**
 * Data Transfer Object (DTO) que representa os detalhes completos de um convite de partida.
 *
 * Este objeto é desenhado para facilitar a interação com formulários, armazenando tanto o
 * objeto temporal [LocalDateTime] (para lógica de negócio) quanto as representações em
 * String separadas ([gameDate] e [gameTime]) para exibição na UI.
 *
 * @property id O identificador único do convite (pode ser null se for um novo convite ainda não persistido).
 * @property idOpponent O identificador da equipa adversária.
 * @property nameOpponent O nome da equipa adversária.
 * @property isHomeGame Indica se o jogo é realizado em casa (true) ou fora (false). Padrão é true.
 * @property gameDate A representação em String da data do jogo (formato: "dd/MM/yyyy").
 * @property gameTime A representação em String da hora do jogo (formato: "HH:mm").
 * @property gameDateTime O objeto [LocalDateTime] real contendo a data e hora do jogo.
 */
data class MatchInviteDto(
    val id: String? = null,
    val idOpponent: String? = null,
    val nameOpponent: String? = null,
    val isHomeGame: Boolean = true,
    val gameDate: String? = null,
    val gameTime: String? = null,
    val gameDateTime: LocalDateTime? = null
) {
    companion object {
        private val dateTimeFormatter =
            DateTimeFormatter.ofPattern(Patterns.DATE_TIME)
        private val dateFormatter =
            DateTimeFormatter.ofPattern("dd/MM/yyyy")
        private val timeFormatter =
            DateTimeFormatter.ofPattern("HH:mm")

        // criar DTO vindo do backend
        fun fromBackend(
            id: String?,
            idOpponent: String?,
            nameOpponent: String?,
            isHomeGame: Boolean,
            gameDateTime: LocalDateTime?
        ): MatchInviteDto {

            return MatchInviteDto(
                id = id,
                idOpponent = idOpponent,
                nameOpponent = nameOpponent,
                isHomeGame = isHomeGame,
                gameDate = gameDateTime?.format(dateFormatter),
                gameTime = gameDateTime?.format(timeFormatter),
                gameDateTime = gameDateTime
            )
        }

        fun genericForCreate(nameOpponent: String): MatchInviteDto {
            return MatchInviteDto(
                id = null,
                idOpponent = null,
                nameOpponent = nameOpponent,
                isHomeGame = true,
                gameDate = null,
                gameTime = null,
                gameDateTime = null
            )
        }

        fun genericForNegotiate(): MatchInviteDto {
            val sampleDateTime = LocalDateTime.now().plusDays(2).withHour(18).withMinute(0) // ex: daqui 2 dias às 18:00
            return MatchInviteDto(
                id = "12345",
                idOpponent = "opponent_001",
                nameOpponent = "FC Barcelona",
                isHomeGame = false,
                gameDate = sampleDateTime.format(dateFormatter),
                gameTime = sampleDateTime.format(timeFormatter),
                gameDateTime = sampleDateTime
            )
        }

        fun combineToDateTime(date: String?, time: String?): LocalDateTime? {
            if (date == null || time == null) return null
            return try {
                LocalDateTime.parse("$date $time", dateTimeFormatter)
            } catch (e: Exception) {
                null
            }
        }
    }
}
