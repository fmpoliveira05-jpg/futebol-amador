package com.example.amfootball.data.remote.dtos.matchInivite

import com.example.amfootball.core.utils.Patterns
import com.example.amfootball.data.remote.dtos.support.TeamDto
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
 */
data class MatchInviteDto(
    @SerializedName("Id", alternate = ["id"])
    val id: String = "",
    @SerializedName("Sender", alternate = ["sender", "Opponent"])
    val opponent: TeamDto = TeamDto(),
    @SerializedName("Receiver", alternate = ["receiver"])
    val receiver: TeamDto = TeamDto(),
    @SerializedName("NamePitch", alternate = ["namePitch"])
    val namePitch: String = "",
    @SerializedName("isHome", alternate = ["IsHome", "homePitch"])
    val isHomeGame: Boolean = true,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val gameDateRaw: String = "",
    val gameDateString: String = "",
    val gameTimeString: String? = "",
) {
    val gameDate: LocalDateTime
        get() {
            return try {
                if (gameDateRaw.isNotBlank()) {
                    LocalDateTime.parse(gameDateRaw, DateTimeFormatter.ISO_LOCAL_DATE_TIME)
                } else {
                    LocalDateTime.now()
                }
            } catch (e: Exception) {
                e.printStackTrace()
                LocalDateTime.now()
            }
        }

    companion object {
        private val dateFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)
        private val timeFormatter = DateTimeFormatter.ofPattern(Patterns.TIME)

        fun createFromBackend(dto: MatchInviteDto): MatchInviteDto {
            val parsedDateTime = dto.gameDate

            return dto.copy(
                gameDateString = parsedDateTime.format(dateFormatter),
                gameTimeString = parsedDateTime.format(timeFormatter)
            )
        }
    }
}
