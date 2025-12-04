package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterTeamActions
import com.example.amfootball.data.actions.itemsList.ItemsListTeamAction
import com.example.amfootball.data.dtos.rank.RankNameDto
import com.example.amfootball.data.dtos.team.ItemTeamInfoDto

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
    val mockFiltersActions = FilterTeamActions(
        {}, {}, {}, {}, {}, {}, {}, {}, {}, ButtonFilterActions({}, {})
    )

    /**
     * Implementação "No-Op" das ações de interação com os itens da lista de equipas.
     *
     * Fornece callbacks vazios para eventos de clique (ver detalhes, desafiar, etc.).
     */
    val mockItemActions = ItemsListTeamAction(
        { _,_, _ -> }, { _, _ -> }, { _, _ -> }
    )
}