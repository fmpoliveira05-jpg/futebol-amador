package com.example.amfootball.data.interfaces.api

import com.example.amfootball.core.utils.Arguments
import com.example.amfootball.core.utils.BaseEndpoints
import com.example.amfootball.data.remote.dtos.postponeMatch.PostPoneResponse
import com.example.amfootball.data.remote.dtos.postponeMatch.PostPoneReturn
import com.example.amfootball.data.remote.dtos.postponeMatch.PostponeDto
import retrofit2.Response
import retrofit2.http.Body
import retrofit2.http.GET
import retrofit2.http.HTTP
import retrofit2.http.POST
import retrofit2.http.Path
import retrofit2.http.QueryMap

interface PostPoneMatchApi {

    @GET("${BaseEndpoints.POSTPONED_MATCH_API}/{${Arguments.ID_TEAM}}/PostPoneMatch")
    suspend fun getPostPoneMatch(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @QueryMap filters: Map<String, String>
    ): Response<List<PostponeDto>>

    @POST("${BaseEndpoints.POSTPONED_MATCH_API}/{${Arguments.ID_TEAM}}/PostPoneMatch/AcceptPostponeMatch")
    suspend fun acceptPostPoneMatch(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @Body postPone: PostPoneResponse
    ): Response<PostPoneReturn>

    @HTTP(
        method = "DELETE",
        path = "${BaseEndpoints.POSTPONED_MATCH_API}/{${Arguments.ID_TEAM}}/PostPoneMatch/RejectPostponeMatch",
        hasBody = true
    )
    suspend fun rejectPostPoneMatch(
        @Path(Arguments.ID_TEAM) idTeam: String,
        @Body postPone: PostPoneResponse
    ): Response<Unit>
}
