package com.example.amfootball.data.interfaces.services

import com.example.amfootball.data.remote.dtos.OpenStreetMapPlace
import retrofit2.http.GET
import retrofit2.http.Header
import retrofit2.http.Query

interface OpenStreetMapService {
    // URL Base será: https://nominatim.openstreetmap.org/

    @GET("search")
    suspend fun verifyAddress(
        @Query("q") address: String,
        @Query("format") format: String = "json",
        @Query("limit") limit: Int = 1,
        @Header("User-Agent") userAgent: String = "AMFootball-App (willkie79@.com)"
    ): List<OpenStreetMapPlace>
}