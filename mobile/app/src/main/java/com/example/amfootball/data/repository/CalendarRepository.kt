package com.example.amfootball.data.repository

import com.example.amfootball.data.dtos.match.CalendarInfoDto
import com.example.amfootball.data.filters.FilterCalendar
import com.example.amfootball.data.network.ApiBackend
import javax.inject.Inject

class CalendarRepository @Inject constructor(
    private val api: ApiBackend
) {

    suspend fun getCalendar(teamId: String, filter: FilterCalendar?): List<CalendarInfoDto> {
        return emptyList()
    }
}
