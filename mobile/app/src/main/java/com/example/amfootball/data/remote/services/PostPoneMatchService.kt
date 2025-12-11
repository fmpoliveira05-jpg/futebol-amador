package com.example.amfootball.data.remote.services

import com.example.amfootball.core.utils.safeApiCallWithNotReturn
import com.example.amfootball.core.utils.safeApiCallWithReturn
import com.example.amfootball.data.filters.FilterPostPoneMatch
import com.example.amfootball.data.filters.toQueryMap
import com.example.amfootball.data.interfaces.api.PostPoneMatchApi
import com.example.amfootball.data.remote.dtos.postponeMatch.PostPoneResponse
import com.example.amfootball.data.remote.dtos.postponeMatch.PostPoneReturn
import com.example.amfootball.data.remote.dtos.postponeMatch.PostponeDto
import javax.inject.Inject

class PostPoneMatchService @Inject constructor(
    private val postPoneMatchApi: PostPoneMatchApi
) {

    suspend fun getPostPoneMatch(teamId: String, filter: FilterPostPoneMatch?): List<PostponeDto> {
        val filters = filter?.toQueryMap() ?: emptyMap()

        return safeApiCallWithReturn {
            postPoneMatchApi.getPostPoneMatch(idTeam = teamId, filters = filters)
        }
    }

    suspend fun acceptPostPoneMatch(teamId: String, postPone: PostPoneResponse): PostPoneReturn {
        return safeApiCallWithReturn {
            postPoneMatchApi.acceptPostPoneMatch(idTeam = teamId, postPone = postPone)
        }
    }

    suspend fun rejectPostPoneMatch(teamId: String, postPone: PostPoneResponse) {
        safeApiCallWithNotReturn {
            postPoneMatchApi.rejectPostPoneMatch(idTeam = teamId, postPone = postPone)
        }
    }
}