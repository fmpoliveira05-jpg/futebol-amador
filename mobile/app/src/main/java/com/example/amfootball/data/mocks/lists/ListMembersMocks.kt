package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMemberTeamAction
import com.example.amfootball.data.actions.itemsList.ItemsListMemberAction
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

object ListMembersMocks {
    val mockListTypes = listOf(null) + TypeMember.values().toList()

    val mockListPositions = listOf(null) + Position.values().toList()

    val mockMembers = listOf(
        MemberTeamDto(
            id = "1",
            name = "João Silva",
            age = 25,
            positionId = 0,
            isAdmin = true,
            image = "",
            height = 185
        ),
        MemberTeamDto(
            id = "2",
            name = "Pedro Santos",
            age = 22,
            positionId = 2,
            isAdmin = false,
            image = "",
            height = 178
        ),
        MemberTeamDto(
            id = "3",
            name = "Miguel Costa",
            age = 28,
            positionId = 1,
            isAdmin = false,
            image = "",
            height = 192
        )
    )

    val mockFilterActions = FilterMemberTeamAction(
        {}, {}, {}, {}, {}, ButtonFilterActions({}, {})
    )

    val mockItemActions = ItemsListMemberAction(
        onPromoteMember = {},
        onDemoteMember = {},
        onRemovePlayer = {},
        onShowMoreInfo = { _, _ -> }
    )

}