package com.example.amfootball.competicao

import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.DELETE
import retrofit2.http.GET
import retrofit2.http.POST
import retrofit2.http.PUT
import retrofit2.http.Path
import retrofit2.http.Query
import retrofit2.http.QueryMap

/** Endpoints das ligas, transferências, onze inicial, relatório do jogo e perfil desportivo. */
interface CompeticaoApi {

    // Ligas e classificação

    @GET("api/leagues")
    suspend fun getLeagues(): Response<List<LeagueDto>>

    @GET("api/Leaderboard")
    suspend fun getStandings(@Query("leagueId") leagueId: String?): Response<StandingsDto>

    @GET("api/leagues/seasons/{seasonId}/fixtures")
    suspend fun getFixtures(@Path("seasonId") seasonId: String): Response<List<FixtureRoundDto>>

    @POST("api/leagues/{leagueId}/register/{teamId}")
    suspend fun registerTeam(
        @Path("leagueId") leagueId: String,
        @Path("teamId") teamId: String
    ): Response<SeasonDto>

    @GET("api/Team/{teamId}/titles")
    suspend fun getTitles(@Path("teamId") teamId: String): Response<List<TeamTitleDto>>

    // Transferências

    @GET("api/transfers/market/{teamId}")
    suspend fun getMarket(
        @Path("teamId") teamId: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<MarketPlayerDto>>

    @POST("api/transfers/listings/{teamId}/{playerId}")
    suspend fun listPlayer(
        @Path("teamId") teamId: String,
        @Path("playerId") playerId: String
    ): Response<Unit>

    @DELETE("api/transfers/listings/{teamId}/{playerId}")
    suspend fun unlistPlayer(
        @Path("teamId") teamId: String,
        @Path("playerId") playerId: String
    ): Response<Unit>

    @POST("api/transfers/offers")
    suspend fun createOffer(@Body offer: CreateOfferDto): Response<TransferOfferDto>

    @GET("api/transfers/offers/team/{teamId}")
    suspend fun getTeamOffers(@Path("teamId") teamId: String): Response<TeamOffersDto>

    @GET("api/transfers/offers/player")
    suspend fun getPlayerOffers(): Response<List<TransferOfferDto>>

    @POST("api/transfers/offers/{offerId}/accept")
    suspend fun acceptOffer(@Path("offerId") offerId: String): Response<TransferOfferDto>

    @POST("api/transfers/offers/{offerId}/reject")
    suspend fun rejectOffer(@Path("offerId") offerId: String): Response<TransferOfferDto>

    // Perfil desportivo

    @GET("api/Player/{playerId}/profile")
    suspend fun getPlayerProfile(@Path("playerId") playerId: String): Response<PerfilJogadorDto>

    @PUT("api/Player/update/{playerId}")
    suspend fun updatePlayer(
        @Path("playerId") playerId: String,
        @Body dto: AtualizarPerfilDto
    ): Response<Unit>

    // Onze inicial e relatório

    @GET("api/lineups/formations")
    suspend fun getFormations(): Response<List<FormationDto>>

    @GET("api/lineups/{matchId}/{teamId}")
    suspend fun getLineup(
        @Path("matchId") matchId: String,
        @Path("teamId") teamId: String
    ): Response<LineupDto>

    @PUT("api/lineups/{matchId}/{teamId}")
    suspend fun saveLineup(
        @Path("matchId") matchId: String,
        @Path("teamId") teamId: String,
        @Body lineup: SaveLineupDto
    ): Response<LineupDto>

    @GET("api/matches/{matchId}/report")
    suspend fun getReport(@Path("matchId") matchId: String): Response<MatchReportDto>

    // Calendário

    @GET("api/Calendar/{teamId}/history")
    suspend fun getCalendarHistory(@Path("teamId") teamId: String): Response<List<CalendarMarkerDto>>

    @PUT("api/Calendar/{teamId}/{matchId}/cancel-reschedule")
    suspend fun cancelAndReschedule(
        @Path("teamId") teamId: String,
        @Path("matchId") matchId: String,
        @Body dto: CancelRescheduleDto
    ): Response<Unit>
}
