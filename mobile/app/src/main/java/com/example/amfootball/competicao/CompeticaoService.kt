package com.example.amfootball.competicao

import com.example.amfootball.core.utils.safeApiCallWithNotReturn
import com.example.amfootball.core.utils.safeApiCallWithReturn
import javax.inject.Inject
import javax.inject.Singleton

/** Acesso à API das funcionalidades de competição, com o tratamento de erros comum da app. */
@Singleton
class CompeticaoService @Inject constructor(
    private val api: CompeticaoApi
) {
    suspend fun ligas(): List<LeagueDto> = safeApiCallWithReturn { api.getLeagues() }

    suspend fun classificacao(leagueId: String?): StandingsDto =
        safeApiCallWithReturn { api.getStandings(leagueId) }

    suspend fun jornadas(seasonId: String): List<FixtureRoundDto> =
        safeApiCallWithReturn { api.getFixtures(seasonId) }

    suspend fun inscrever(leagueId: String, teamId: String): SeasonDto =
        safeApiCallWithReturn { api.registerTeam(leagueId, teamId) }

    suspend fun titulos(teamId: String): List<TeamTitleDto> =
        safeApiCallWithReturn { api.getTitles(teamId) }

    suspend fun mercado(teamId: String, filtros: FiltrosMercado): List<MarketPlayerDto> =
        safeApiCallWithReturn { api.getMarket(teamId, filtros.paraQuery()) }

    suspend fun colocarNoMercado(teamId: String, playerId: String) =
        safeApiCallWithNotReturn { api.listPlayer(teamId, playerId) }

    suspend fun retirarDoMercado(teamId: String, playerId: String) =
        safeApiCallWithNotReturn { api.unlistPlayer(teamId, playerId) }

    suspend fun fazerProposta(teamId: String, playerId: String, mensagem: String?): TransferOfferDto =
        safeApiCallWithReturn { api.createOffer(CreateOfferDto(teamId, playerId, mensagem)) }

    suspend fun propostasEquipa(teamId: String): TeamOffersDto =
        safeApiCallWithReturn { api.getTeamOffers(teamId) }

    suspend fun propostasJogador(): List<TransferOfferDto> =
        safeApiCallWithReturn { api.getPlayerOffers() }

    suspend fun aceitarProposta(offerId: String): TransferOfferDto =
        safeApiCallWithReturn { api.acceptOffer(offerId) }

    suspend fun recusarProposta(offerId: String): TransferOfferDto =
        safeApiCallWithReturn { api.rejectOffer(offerId) }

    suspend fun perfil(playerId: String): PerfilJogadorDto =
        safeApiCallWithReturn { api.getPlayerProfile(playerId) }

    suspend fun atualizarPerfil(playerId: String, dto: AtualizarPerfilDto) =
        safeApiCallWithNotReturn { api.updatePlayer(playerId, dto) }

    suspend fun taticas(): List<FormationDto> = safeApiCallWithReturn { api.getFormations() }

    suspend fun onze(matchId: String, teamId: String): LineupDto =
        safeApiCallWithReturn { api.getLineup(matchId, teamId) }

    suspend fun guardarOnze(matchId: String, teamId: String, onze: SaveLineupDto): LineupDto =
        safeApiCallWithReturn { api.saveLineup(matchId, teamId, onze) }

    suspend fun relatorio(matchId: String): MatchReportDto =
        safeApiCallWithReturn { api.getReport(matchId) }

    suspend fun historicoCalendario(teamId: String): List<CalendarMarkerDto> =
        safeApiCallWithReturn { api.getCalendarHistory(teamId) }

    suspend fun cancelarERemarcar(teamId: String, matchId: String, motivo: String, novaData: String) =
        safeApiCallWithNotReturn {
            api.cancelAndReschedule(teamId, matchId, CancelRescheduleDto(motivo, novaData))
        }
}
