package com.example.amfootball.data.dtos.match

import com.example.amfootball.data.dtos.support.PitchInfo
import com.example.amfootball.data.dtos.support.TeamStatisticsDto
import com.example.amfootball.data.enums.MatchStatus
import com.example.amfootball.data.enums.MatchResult
import com.example.amfootball.data.enums.TypeMatch
import com.example.amfootball.utils.Patterns
import com.google.gson.annotations.SerializedName
import java.time.LocalDateTime
import java.time.format.DateTimeFormatter

/**
 * Data Transfer Object (DTO) que representa as informações resumidas de uma partida
 * para visualização num calendário ou lista de histórico.
 *
 * Esta classe foi desenhada para receber tipos primitivos (String, Int, Boolean) da API
 * para evitar erros de conversão do Gson (ex: datas ou enums), convertendo-os internamente
 * para objetos de domínio através de propriedades computadas.
 *
 * @property idMatch O identificador único da partida.
 * @property matchStatusId O valor inteiro "bruto" do estado da partida recebido da API (0, 1, 2...).
 * @property rawGameDate A data da partida em formato String (ISO-8601) recebida da API.
 * @property typeMatchBool O booleano recebido da API que indica se é competitivo (true) ou casual (false).
 * @property matchResultId O valor inteiro "bruto" do resultado da partida recebido da API.
 * @property team As estatísticas e informações da equipa principal (ou equipa do utilizador).
 * @property opponent As estatísticas e informações da equipa adversária.
 * @property isHome Indica se o jogo é em casa (true) ou fora (false).
 * @property pitchGame Informações detalhadas sobre o campo/local onde o jogo é disputado.
 *
 * @property matchStatus Propriedade computada que converte [matchStatusId] para o Enum [MatchStatus].
 * @property gameDate Propriedade computada que converte [rawGameDate] para [LocalDateTime].
 * @property formattedDate Data formatada para exibição na UI (ex: "dd/MM/yyyy HH:mm").
 * @property matchResult Propriedade computada que converte [matchResultId] para o Enum [MatchResult].
 * @property typeMatch Propriedade computada que converte [typeMatchBool] para o Enum [TypeMatch].
 */
data class InfoMatchCalendar(
    val idMatch: String,
    @SerializedName("MatchStatus", alternate = ["matchStatus"])
    val matchStatusId: Int = 0,
    @SerializedName("GameDate", alternate = ["gameDate"])
    val rawGameDate: String = "",
    @SerializedName("IsCompetitive", alternate = ["isCompetitive"])
    val typeMatchBool: Boolean = false,
    @SerializedName("MatchResult", alternate = ["matchResult"])
    val matchResultId: Int = 0,
    @SerializedName("Team", alternate = ["team"])
    val team: TeamStatisticsDto,
    @SerializedName("Opponent", alternate = ["opponent"])
    val opponent: TeamStatisticsDto,
    @SerializedName("IsHome", alternate = ["isHome"])
    val isHome: Boolean = false,
    @SerializedName("PitchGame", alternate = ["pitchGame"])
    val pitchGame: PitchInfo
) {
    /**
     * Converte a String bruta da API para um objeto [LocalDateTime].
     * Caso falhe a conversão (ex: string vazia ou formato inválido), retorna a data/hora atual.
     */
    val gameDate: LocalDateTime
        get() = try {
            LocalDateTime.parse(rawGameDate)
        } catch (e: Exception) {
            LocalDateTime.now()
        }

    /**
     * Formata a [gameDate] para ser apresentada na UI conforme o padrão [Patterns.DATE_TIME].
     * Ex: "19/11/2025 22:24"
     */
    val formattedDate: String
        get() = try {
            gameDate.format(DateTimeFormatter.ofPattern(Patterns.DATE_TIME))
        } catch (e: Exception) {
            rawGameDate // Fallback
        }

    /**
     * Converte o inteiro [matchResultId] para o Enum [MatchResult].
     * Mapeamento: 0 -> WIN, 1 -> DRAW, 2 -> LOSE.
     */
    val matchResult: MatchResult
        get() = when(matchResultId) {
            0 -> MatchResult.WIN
            1 -> MatchResult.DRAW
            2 -> MatchResult.LOSE
            else -> MatchResult.UNDEFINED
        }

    /**
     * Converte o inteiro [matchStatusId] para o Enum [MatchStatus].
     * Mapeamento: 0 -> SCHEDULED, 1 -> IN_PROGRESS, 2 -> DONE, etc.
     */
    val matchStatus: MatchStatus
        get() = when(matchStatusId) {
            0 -> MatchStatus.SCHEDULED
            1 -> MatchStatus.IN_PROGRESS
            2 -> MatchStatus.DONE
            3 -> MatchStatus.POST_PONED
            else -> MatchStatus.CANCELED
        }

    /**
     * Converte o booleano [typeMatchBool] para o Enum [TypeMatch].
     * Mapeamento: false -> CASUAL, true -> COMPETITIVE.
     */
    val typeMatch: TypeMatch
        get() = when(typeMatchBool) {
            false -> TypeMatch.CASUAL
            else -> TypeMatch.COMPETITIVE
        }
}
