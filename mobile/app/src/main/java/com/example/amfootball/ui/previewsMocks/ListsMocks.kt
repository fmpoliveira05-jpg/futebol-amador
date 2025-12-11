package com.example.amfootball.ui.previewsMocks

import androidx.compose.runtime.mutableStateOf
import com.example.amfootball.data.remote.dtos.leadboard.InfoTeamLeadboard
import com.example.amfootball.data.remote.dtos.leadboard.LeadboardDto
import com.example.amfootball.data.remote.dtos.membershipRequest.MembershipRequestInfoDto
import com.example.amfootball.data.remote.dtos.player.InfoPlayerDto
import com.example.amfootball.data.remote.dtos.rank.RankNameDto
import com.example.amfootball.data.remote.dtos.team.ItemTeamInfoDto
import com.example.amfootball.domains.enums.Position
import com.example.amfootball.ui.actions.filters.ButtonFilterActions
import com.example.amfootball.ui.actions.filters.FilterListPlayersActions
import com.example.amfootball.ui.actions.filters.FilterMemberShipRequestActions
import com.example.amfootball.ui.actions.itemsList.ItemsMemberShipRequest
import com.example.amfootball.ui.actions.lists.LeadBoardActions

/**
 * Fornece dados fictícios (Mocks) e stubs de ações para o ecrã de Pesquisa/Listagem de Jogadores Globais.
 *
 * Este objeto é utilizado para popular as pré-visualizações (Previews) e testes de UI, permitindo validar
 * o layout dos cartões de jogador e a barra de filtros sem dependência da API.
 */
object ListPlayersMocks {
    /**
     * Lista estática de jogadores com perfis variados para testes visuais.
     *
     * **Cenários incluídos:**
     * 1. **Jogador Com Equipa (Messi):** `haveTeam = true`. Útil para verificar se a UI bloqueia convites ou mostra a afiliação atual.
     * 2. **Jogador Livre (Bernardo Silva):** `haveTeam = false`. Útil para validar o fluxo de convite ou contratação.
     *
     * Também varia atributos físicos (altura) e geográficos (morada) para testar a formatação do cartão.
     */
    val list = listOf(
        InfoPlayerDto(
            id = "1",
            name = "Lionel Messi",
            age = 36,
            address = "Miami, USA",
            heigth = 170,
            position = Position.FORWARD,
            haveTeam = true,
            image = ""
        ),
        InfoPlayerDto(
            id = "2",
            name = "Bernardo Silva",
            age = 29,
            address = "Manchester, UK",
            heigth = 173,
            position = Position.MIDFIELDER,
            haveTeam = false,
            image = ""
        )
    )

    /**
     * Implementação "No-Op" (No Operation) das ações de filtro da lista de jogadores.
     *
     * Define callbacks vazios `{}` para todos os campos de critério (Nome, Cidade, Idade, Altura, Posição),
     * permitindo renderizar a interface de filtros complexa sem lógica de negócio associada.
     */
    val Actions =
        FilterListPlayersActions(
            onNameChange = {},
            onCityChange = {},
            onMinAgeChange = {},
            onMaxAgeChange = {},
            onPositionChange = {},
            onMinSizeChange = {},
            onMaxSizeChange = {},
            buttonActions = ButtonFilterActions(
                onFilterApply = {},
                onFilterClean = {}
            )
        )

    /**
     * Lista de opções para o filtro de "Posição", preparada para componentes de seleção (Dropdown/Spinner).
     *
     * Inicia com `null` para representar a opção "Todas as Posições" ou "Indiferente",
     * seguida de todos os valores definidos no enum [Position].
     */
    val Positions = listOf(null) + Position.values().toList()
}

/**
 * Fornece dados fictícios (Mocks) e stubs de ações para o ecrã de Listagem/Pesquisa de Equipas.
 *
 * Este objeto é utilizado para testes de UI e Previews, permitindo validar a renderização dos
 * cartões de equipa, a correta formatação de estatísticas (pontos, idade média) e o funcionamento
 * visual dos filtros de Rank.
 */
object ListTeamMocks {
    /**
     * Lista de níveis de classificação (Ranks) disponíveis para filtragem.
     *
     * Útil para popular componentes de seleção (Dropdowns/Spinners) nos filtros de pesquisa.
     * Contém: Bronze, Prata e Ouro.
     */
    val mockRanks = listOf(
        RankNameDto(id = "1", name = "Bronze"),
        RankNameDto(id = "2", name = "Prata"),
        RankNameDto(id = "3", name = "Ouro")
    )

    /**
     * Lista estática de equipas com perfis variados para testes visuais.
     *
     * **Cenários incluídos:**
     * 1. **Alta Competição ("Lisboa Lions"):** Rank Ouro, muitos pontos e membros.
     * 2. **Universitária ("Porto Pirates"):** Rank Prata, idade média jovem.
     * 3. **Formação ("Braga Warriors"):** Rank Bronze, menos pontos e membros.
     *
     * Estes dados permitem verificar se a UI lida corretamente com diferentes comprimentos de texto
     * e formatações numéricas (ex: `averageAge` como Double).
     */
    val mockTeams = listOf(
        ItemTeamInfoDto(
            id = "1",
            name = "Lisboa Lions",
            fullAddress = "Rua Principal, Lisboa",
            rank = RankNameDto(id = "3", name = "Ouro"),
            points = 1500,
            numberMembers = 30,
            averageAge = 25.3,
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

    /**
     * Implementação "No-Op" (No Operation) das ações de filtro de equipas.
     *
     * Define callbacks vazios `{}` para os múltiplos critérios de filtragem (Nome, Rank, Localização, etc.),
     * permitindo renderizar a interface de filtros complexa sem lógica de negócio.
     */
    val mockFiltersActions =
        _root_ide_package_.com.example.amfootball.ui.actions.filters.FilterTeamActions(
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            {},
            _root_ide_package_.com.example.amfootball.ui.actions.filters.ButtonFilterActions({}, {})
        )

    /**
     * Implementação "No-Op" das ações de interação com os itens da lista de equipas.
     *
     * Fornece callbacks vazios para eventos de clique (ver detalhes, desafiar, etc.).
     */
    val mockItemActions =
        _root_ide_package_.com.example.amfootball.ui.actions.itemsList.ItemsListTeamAction(
            { _, _, _ -> }, { _, _ -> }, { _, _ -> }
        )
}

object ListMemberShipRequestMocks {
    val filterActions = FilterMemberShipRequestActions(
        onSenderNameChange = {},
        onMinDateSelected = {},
        onMaxDateSelected = {},
        buttonActions = ButtonFilterActions(
            onFilterApply = {},
            onFilterClean = {}
        ),
    )

    val itemsActions = ItemsMemberShipRequest(
        acceptMemberShipRequest = { _, _, _, _ -> },
        rejectMemberShipRequest = { _, _, _, -> },
        showMore = { _, _, _ -> }
    )

    val mockRequests = MembershipRequestInfoDto.generateMemberShipRequestPlayer()
}

object LeadboardMocks {
    val fakeList = listOf(
        LeadboardDto(
            position = 1,
            team = InfoTeamLeadboard(
                id = "1",
                name = "Porto Lions",
                currentPoints = 1250,
                nameRank = "Elite",
                logoTeam = null
            )
        ),
        LeadboardDto(
            position = 2,
            team = InfoTeamLeadboard(
                id = "2",
                name = "Lisboa Navigators",
                currentPoints = 980,
                nameRank = "Pro",
                logoTeam = null
            )
        ),
        LeadboardDto(
            position = 3,
            team = InfoTeamLeadboard(
                id = "3",
                name = "Braga Warriors",
                currentPoints = 450,
                nameRank = "Amateur",
                logoTeam = null
            )
        )
    )

    val showMoreState = true
    val fakeActions = LeadBoardActions(
        onShowMore = { _, _ -> },
        isValidShowMoreTeams = { mutableStateOf(showMoreState) },
        onLoadMoreTeams = {}
    )
}