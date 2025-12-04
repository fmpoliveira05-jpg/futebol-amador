package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterTeamActions
import com.example.amfootball.data.actions.itemsList.ItemsListTeamAction
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto

object ListTeamMocks {
    val mockRanks = listOf(
        RankNameDto(id = "1", name = "Bronze"),
        RankNameDto(id = "2", name = "Prata"),
        RankNameDto(id = "3", name = "Ouro")
    )

    val mockTeams = listOf(
        ItemTeamInfoDto(
            id = "1",
            name = "Lisboa Lions",
            fullAddress = "Rua Principal, Lisboa",
            rank = RankNameDto(id = "3", name = "Ouro"),
            points = 1500,
            numberMembers = 30,
            averageAge = 25.3, // Double correto
            description = "Equipa focada em competição de alto nível.",
            logoTeam = null
        ),
        ItemTeamInfoDto(
            id = "2",
            name = "Porto Pirates",
            fullAddress = "Avenida dos Aliados, Porto",
            rank = RankNameDto(id = "2", name = "Prata"),
            points = 1200,
            numberMembers = 25,
            averageAge = 24.1,
            description = "Equipa universitária do Porto.",
            logoTeam = null
        ),
        ItemTeamInfoDto(
            id = "3",
            name = "Braga Warriors",
            fullAddress = "Estádio Municipal, Braga",
            rank = RankNameDto(id = "1", name = "Bronze"),
            points = 800,
            numberMembers = 20,
            averageAge = 22.2,
            description = "Formação de novos talentos.",
            logoTeam = null
        )
    )

    val mockFiltersActions = FilterTeamActions(
        {}, {}, {}, {}, {}, {}, {}, {}, {}, ButtonFilterActions({}, {})
    )

    val mockItemActions = ItemsListTeamAction(
        { _,_, _ -> }, { _, _ -> }, { _, _ -> }
    )

}