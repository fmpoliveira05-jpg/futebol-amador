package com.example.amfootball.data.dtos.matchInivite

import com.example.amfootball.data.dtos.support.TeamDto
import com.example.amfootball.utils.Patterns
import com.google.gson.annotations.SerializedName
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
 * @property opponent guarda os dados do opponent, nome e id.
 * @property isHomeGame Indica se o jogo é realizado em casa (true) ou fora (false). Padrão é true.
 * @property gameDate A representação em String da data do jogo (formato: "dd/MM/yyyy").
 * @property gameTime A representação em String da hora do jogo (formato: "HH:mm").
 * @property gameDateTime O objeto [LocalDateTime] real contendo a data e hora do jogo.
 * @property isCompetitive Indica se o jogo é competitivo (true) ou casual (false). Padrão é false.
 */
data class MatchInviteDto(
    @SerializedName("", alternate = ["idMatch"])
    val id: String? = null,
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamDto? = null,
    @SerializedName("IsHome", alternate = ["isHome"])
    val isHomeGame: Boolean = true,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val gameDateRaw: String? = null,
    val gameDateString: String? = null,
    val gameTimeString: String? = null,
    @SerializedName("IsCompetitive", alternate = ["isCompetitive"])
    val isCompetitive: Boolean? = false
) {
    companion object {
        private val dateFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)
        private val timeFormatter = DateTimeFormatter.ofPattern(Patterns.TIME)
        private val apiParser = DateTimeFormatter.ISO_LOCAL_DATE_TIME

        fun createFromBackend(dto: MatchInviteDto): MatchInviteDto {
            val parsedDateTime: LocalDateTime? = try {
                if (!dto.gameDateRaw.isNullOrBlank()) {
                    LocalDateTime.parse(dto.gameDateRaw, apiParser)
                } else {
                    null
                }
            } catch (e: Exception) {
                e.printStackTrace()
                null
            }

            return dto.copy(
                gameDateString = parsedDateTime?.format(dateFormatter),
                gameTimeString = parsedDateTime?.format(timeFormatter)
            )
        }

        fun createEmpty(): MatchInviteDto = MatchInviteDto()
    }
}
