package com.example.amfootball.competicao

/*
 * DTOs das ligas, transferências, onze inicial, relatório do jogo e perfil do jogador.
 * Seguem o contrato em docs/novas-funcionalidades.md: a API serializa em camelCase, os enums
 * como inteiros e as datas como texto ISO 8601 (convertidas com [Datas]).
 */

// ---------- Ligas e classificação ----------

data class SeasonDto(
    val id: String = "",
    val leagueId: String = "",
    val name: String = "",
    /** 0 = inscrições, 1 = a decorrer, 2 = terminada. */
    val status: Int = 0,
    val startDate: String? = null,
    val endDate: String? = null,
    val teamCount: Int = 0
)

data class LeagueDto(
    val id: String = "",
    val name: String = "",
    val level: Int = 1,
    val promotionSpots: Int = 0,
    val relegationSpots: Int = 0,
    val seasonDurationDays: Int = 0,
    val trophyName: String = "",
    val teamCount: Int = 0,
    val currentSeason: SeasonDto? = null
)

data class StandingRowDto(
    val position: Int = 0,
    val teamId: String = "",
    val teamName: String = "",
    val icon: String? = null,
    val played: Int = 0,
    val won: Int = 0,
    val drawn: Int = 0,
    val lost: Int = 0,
    val goalsFor: Int = 0,
    val goalsAgainst: Int = 0,
    val goalDifference: Int = 0,
    val points: Int = 0,
    /** Últimos resultados ("V", "E", "D"), do mais antigo para o mais recente. */
    val form: List<String>? = null,
    /** "PROMOTION", "RELEGATION" ou nulo. */
    val zone: String? = null
)

data class StandingsDto(
    val league: LeagueDto? = null,
    val season: SeasonDto? = null,
    val rows: List<StandingRowDto>? = null
)

data class FixtureMatchDto(
    val idMatch: String = "",
    val date: String? = null,
    val status: Int = 0,
    val homeTeamId: String = "",
    val homeTeamName: String = "",
    val awayTeamId: String = "",
    val awayTeamName: String = "",
    val homeGoals: Int? = null,
    val awayGoals: Int? = null
)

data class FixtureRoundDto(
    val round: Int = 0,
    val matches: List<FixtureMatchDto>? = null
)

data class TeamTitleDto(
    val trophyName: String = "",
    val leagueName: String = "",
    val count: Int = 0,
    val seasons: List<String>? = null
)

// ---------- Transferências ----------

data class MarketPlayerDto(
    val playerId: String = "",
    val name: String = "",
    val age: Int = 0,
    val position: Int = 0,
    val nationality: String? = null,
    val imageUrl: String? = null,
    val teamId: String? = null,
    val teamName: String? = null,
    val leagueId: String? = null,
    val leagueName: String? = null,
    val isListed: Boolean = false
)

data class TransferOfferDto(
    val id: String = "",
    val playerId: String = "",
    val playerName: String = "",
    val fromTeamId: String? = null,
    val fromTeamName: String? = null,
    val toTeamId: String = "",
    val toTeamName: String = "",
    /** 0 = à espera do clube, 1 = à espera do jogador, 2 = aceite, 3 = recusada, 4 = cancelada. */
    val status: Int = 0,
    val message: String? = null,
    val createdAt: String? = null,
    val decidedAt: String? = null
)

data class TeamOffersDto(
    val received: List<TransferOfferDto>? = null,
    val sent: List<TransferOfferDto>? = null
)

data class CreateOfferDto(
    val teamId: String,
    val playerId: String,
    val message: String?
)

// ---------- Perfil do jogador ----------

data class TeamRefDto(
    val idTeam: String = "",
    val name: String = ""
)

data class StatsLineDto(
    val season: String? = null,
    val teamId: String? = null,
    val teamName: String? = null,
    val games: Int = 0,
    val goals: Int = 0,
    val assists: Int = 0,
    val minutes: Int = 0,
    val yellowCards: Int = 0,
    val redCards: Int = 0
)

data class TransferHistoryDto(
    val date: String? = null,
    val fromTeamName: String? = null,
    val toTeamName: String? = null,
    /** "TRANSFERENCIA", "ADESAO" ou "SAIDA". */
    val kind: String = ""
)

/** Perfil desportivo (GET /api/Player/{id}/profile). Não confundir com o PlayerProfileDto da sessão. */
data class PerfilJogadorDto(
    val id: String = "",
    val name: String = "",
    val imageUrl: String? = null,
    val dateOfBirth: String? = null,
    val age: Int = 0,
    val position: Int = 0,
    val height: Int = 0,
    val weight: Int? = null,
    val preferredFoot: Int? = null,
    val status: Int = 0,
    val nationality: String? = null,
    val countryOfBirth: String? = null,
    val currentTeam: TeamRefDto? = null,
    val joinedTeamAt: String? = null,
    val isListed: Boolean = false,
    val totals: StatsLineDto? = null,
    val career: List<StatsLineDto>? = null,
    val transfers: List<TransferHistoryDto>? = null
)

/** Corpo do PUT /api/Player/update/{id} com os campos novos do perfil. */
data class AtualizarPerfilDto(
    val name: String,
    val dateOfBirth: String?,
    val address: String,
    val email: String?,
    val phone: String?,
    val position: Int,
    val height: Int,
    val weight: Int?,
    val preferredFoot: Int?,
    val status: Int?,
    val nationality: String?,
    val countryOfBirth: String?
)

// ---------- Onze inicial ----------

data class FormationSlotDto(
    val slot: Int = 0,
    val positionCode: String = "",
    /** Enum Position: 0 avançado, 1 médio, 2 defesa, 3 guarda-redes. */
    val role: Int = 0,
    val x: Double = 50.0,
    val y: Double = 50.0
)

data class FormationDto(
    val code: String = "",
    val slots: List<FormationSlotDto>? = null
)

data class LineupStarterDto(
    val slot: Int = 0,
    val positionCode: String = "",
    val playerId: String? = null,
    val playerName: String? = null,
    val position: Int = 0
)

data class LineupBenchDto(
    val playerId: String = "",
    val playerName: String = "",
    val position: Int = 0
)

data class LineupDto(
    val matchId: String = "",
    val teamId: String = "",
    val formation: String = "",
    val deadline: String? = null,
    val isLocked: Boolean = false,
    val isAutoFilled: Boolean = false,
    val exists: Boolean = false,
    val starters: List<LineupStarterDto>? = null,
    val bench: List<LineupBenchDto>? = null
)

data class SaveStarterDto(
    val slot: Int,
    val playerId: String
)

data class SaveLineupDto(
    val formation: String,
    val starters: List<SaveStarterDto>,
    val bench: List<String>
)

// ---------- Eventos e relatório do jogo ----------

data class GoalEventDto(
    /** Nulo quando não se sabe quem marcou (por exemplo, autogolo). */
    val scorerId: String?,
    val assistId: String?,
    val minute: Int
)

data class CardEventDto(
    val playerId: String,
    /** 0 = amarelo, 1 = vermelho. */
    val type: Int,
    val minute: Int
)

data class SubstitutionEventDto(
    val playerOutId: String,
    val playerInId: String,
    val minute: Int
)

data class MatchEventsDto(
    val fouls: Int = 0,
    val goals: List<GoalEventDto> = emptyList(),
    val cards: List<CardEventDto> = emptyList(),
    val substitutions: List<SubstitutionEventDto> = emptyList()
)

data class ReportEventDto(
    /** "GOAL", "YELLOW_CARD", "RED_CARD" ou "SUBSTITUTION". */
    val type: String = "",
    val minute: Int = 0,
    val playerId: String? = null,
    val playerName: String? = null,
    val relatedPlayerId: String? = null,
    val relatedPlayerName: String? = null
)

data class TeamReportDto(
    val teamId: String = "",
    val teamName: String = "",
    val goals: Int? = null,
    val fouls: Int = 0,
    val yellowCards: Int = 0,
    val redCards: Int = 0,
    val substitutions: Int = 0,
    val lineup: LineupDto? = null,
    val events: List<ReportEventDto>? = null
)

data class MatchReportDto(
    val matchId: String = "",
    val date: String? = null,
    val status: Int = 0,
    val isCompetitive: Boolean = false,
    val leagueName: String? = null,
    val round: Int? = null,
    val pitchName: String? = null,
    val home: TeamReportDto? = null,
    val away: TeamReportDto? = null
)

// ---------- Calendário ----------

data class CalendarMarkerDto(
    val idMatch: String = "",
    val date: String? = null,
    /** "CANCELLED" ou "POSTPONED". */
    val kind: String = "",
    val reason: String? = null,
    val opponentName: String? = null,
    val newDate: String? = null
)

data class CancelRescheduleDto(
    val reason: String,
    val newDate: String
)
