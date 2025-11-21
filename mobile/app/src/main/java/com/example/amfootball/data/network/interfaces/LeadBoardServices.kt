package com.example.amfootball.data.network.interfaces

import com.example.amfootball.data.dtos.leadboard.InfoTeamLeadboard
import retrofit2.Response
import retrofit2.http.GET

interface LeadBoardServices {
    @GET("api/")
    suspend fun getLeadBoard(): Response<InfoTeamLeadboard>
}