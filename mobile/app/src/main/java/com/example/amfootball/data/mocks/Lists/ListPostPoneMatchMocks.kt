package com.example.amfootball.data.mocks.Lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPostPoneMatchActions
import com.example.amfootball.data.actions.itemsList.ItemsListPostPoneMatchActions
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.dtos.support.TeamDto
import java.time.LocalDateTime

object ListPostPoneMatchMocks {
    val mockFilterActions = FilterListPostPoneMatchActions(
        {}, {}, {}, {}, {}, {}, ButtonFilterActions({}, {})
    )

    val mockItemActions = ItemsListPostPoneMatchActions(
        acceptPostPoneMatch = {},
        rejectPostPoneMatch = {},
        showMoreInfo = { _, _ -> }
    )

    val mockDate = LocalDateTime.now()

    val mockPostPoneMatches = listOf(
        PostPoneMatchDto(
            id = "1",
            opponent = TeamDto(
                id = "10",
                name = "Dragons FC",
                image = ""
            ),
            gameDate = mockDate.plusDays(1),
            postPoneDate = mockDate.plusDays(2),
            pitchMatch = "Estádio Municipal"
        ),
        PostPoneMatchDto(
            id = "2",
            opponent = TeamDto(
                id = "11",
                name = "Lions Club",
                image = ""
            ),
            gameDate = mockDate.plusDays(5),
            postPoneDate = mockDate.plusDays(7),
            pitchMatch = "Arena Central"
        )
    )
}