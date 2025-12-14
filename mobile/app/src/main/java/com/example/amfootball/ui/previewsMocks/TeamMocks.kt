package com.example.amfootball.ui.previewsMocks

import com.example.amfootball.core.extensions.toUiString
import com.example.amfootball.data.remote.dtos.match.InfoMatchCalendar
import com.example.amfootball.data.remote.dtos.matchInivite.MatchInviteDto
import com.example.amfootball.data.remote.dtos.player.MemberTeamDto
import com.example.amfootball.data.remote.dtos.postponeMatch.PostponeDto
import com.example.amfootball.data.remote.dtos.support.PitchInfo
import com.example.amfootball.data.remote.dtos.support.TeamDto
import com.example.amfootball.data.remote.dtos.support.TeamStatisticsDto
import com.example.amfootball.data.remote.dtos.team.FormTeamDto
import com.example.amfootball.data.remote.dtos.team.ProfileTeamDto
import com.example.amfootball.domains.enums.Position
import com.example.amfootball.domains.enums.TypeMember
import com.example.amfootball.ui.actions.lists.ShowMoreItensAction
import com.example.amfootball.ui.previewsMocks.CalendarMocks.filterActions
import com.example.amfootball.ui.previewsMocks.CalendarMocks.itemActions
import com.example.amfootball.ui.previewsMocks.CalendarMocks.listNormal
import kotlinx.coroutines.flow.MutableStateFlow
import java.time.LocalDateTime
import javax.inject.Singleton

@Singleton
object ListMatchInviteMocks {
    val mockItemsListActions =
        _root_ide_package_.com.example.amfootball.ui.actions.itemsList.ItemListMatchIniviteActions(
            acceptMatchInvite = {},
            rejectMatchInvite = {},
            negociateMatchInvite = { _, _ -> },
            showMoreDetails = { _, _ -> }
        )

    val mockList = listOf(
        MatchInviteDto(
            id = "1",
            opponent = TeamDto(
                id = "opp1",
                name = "Dragões de Leiria",
                image = ""
            ),
            namePitch = "Estádio Municipal",
            gameDateRaw = "2023-12-25T15:30:00",
            isHomeGame = true
        ),
        MatchInviteDto(
            id = "2",
            opponent = TeamDto(
                id = "opp2",
                name = "Águias do Norte",
                image = ""
            ),
            namePitch = "Campo da Tapadinha",
            gameDateRaw = "2023-12-30T21:00:00",
            isHomeGame = false
        )
    )

    val mockFilterActions =
        _root_ide_package_.com.example.amfootball.ui.actions.filters.FilterMatchInviteActions(
            onSenderNameChange = {},
            onMinDateSelected = {},
            onMaxDateSelected = {},
            buttonActions = _root_ide_package_.com.example.amfootball.ui.actions.filters.ButtonFilterActions(
                onFilterApply = {},
                onFilterClean = {}
            )
        )
}

object ProfileTeamMocks {
    val dummyPitch = PitchInfo(
        name = "Estádio D. Afonso Henriques",
        address = "Guimarães",
    )

    val dummyTeam = ProfileTeamDto(
        id = "1",
        name = "Vitória SC",
        description = "O Conquistador. Clube histórico de Portugal.",
        foundationDate = "1922",
        rank = "Ouro",
        totalPoints = 350,
        logo = "",
        pitch = dummyPitch,
    )
}

object EditTeamMocks {
    val mockEditTeam = FormTeamDto(
        name = "Vitória SC",
        description = "Os Conquistadores. A maior equipa do Minho.",
        pitch = PitchInfo(
            name = "Estádio D. Afonso Henriques",
            address = "Praça 26 de Maio, Guimarães"
        )
    )

    val mockActions = _root_ide_package_.com.example.amfootball.ui.actions.forms.FormTeamActions(
        {}, {}, {}, {}, {}
    )

}


/**
 * Fornece dados fictícios (Mocks) e implementações vazias (Stubs) de ações para o ecrã de Calendário de Jogos.
 *
 * Este objeto é utilizado para popular as pré-visualizações do Jetpack Compose (Previews) e para testes de UI,
 * isolando a interface da lógica de negócio real.
 *
 * @property filterActions Uma implementação "No-Op" (No Operation) das ações de filtro.
 * Todos os callbacks são definidos como funções vazias `{}`, permitindo renderizar a UI de filtros
 * sem causar erros de *NullPointer* ou disparar lógica indesejada ao interagir com os campos.
 *
 * @property itemActions Uma implementação "No-Op" das ações dos itens da lista.
 * Permite renderizar os cartões de jogo e simular cliques (Cancelar, Adiar, Iniciar) sem efeitos colaterais.
 *
 * @property listNormal Retorna uma lista dinâmica de [InfoMatchCalendar] para teste.
 * Utiliza um *getter* personalizado para gerar as datas (`LocalDateTime.now()`) no momento do acesso,
 * garantindo que os dados de teste (jogos passados vs futuros) permanecem cronologicamente corretos
 * independentemente do dia em que os testes são executados.
 *
 * **Cenários incluídos na lista:**
 * 1. Jogo Passado ("DONE"): Realizado há 3 dias, jogado "Fora", derrota (1-3).
 * 2. Jogo Futuro ("SCHEDULED"): Agendado para daqui a 5 dias, jogo em "Casa".
 */
object CalendarMocks {
    val filterActions =
        _root_ide_package_.com.example.amfootball.ui.actions.filters.FilterCalendarActions(
            onNameChange = {},
            onMinDateGameChange = {},
            onMaxDateGameChange = {},
            onGameLocalChange = {},
            onTypeMatchChange = {},
            onFinishMatch = {},
            onButtonFilterActions = _root_ide_package_.com.example.amfootball.ui.actions.filters.ButtonFilterActions(
                onFilterApply = {},
                onFilterClean = {}
            )
        )

    val itemActions =
        _root_ide_package_.com.example.amfootball.ui.actions.itemsList.ItemsCalendarActions(
            onCancelMatch = { _ -> },
            onPostPoneMatch = { _ -> },
            onStartMatch = {_, _ ->},
            onFinishMatch = { _, _ -> }
        )

    val listNormal: List<InfoMatchCalendar>
        get() = listOf(
            InfoMatchCalendar(
                idMatch = "1",
                matchStatusId = 2,
                rawGameDate = LocalDateTime.now().minusDays(3).toString(),
                typeMatchBool = true,
                matchResultId = 0,
                pitchGame = PitchInfo("Estádio Municipal", "Rua Principal"),
                team = TeamStatisticsDto("t1", "Vitória SC", 3, ""),
                opponent = TeamStatisticsDto("t2", "Dragões FC", 1, ""),
                isHome = false
            ),
            InfoMatchCalendar(
                idMatch = "2",
                matchStatusId = 0,
                rawGameDate = LocalDateTime.now().plusDays(5).toString(),
                typeMatchBool = false,
                matchResultId = -1,
                pitchGame = PitchInfo("Campo de Treinos", "Avenida Secundária"),
                team = TeamStatisticsDto("t1", "Vitória SC", 0, ""),
                opponent = TeamStatisticsDto("t3", "Sporting CP", 0, ""),
                isHome = true
            )
        )
}

/**
 * Fornece dados fictícios (Mocks) e implementações vazias (Stubs) para o ecrã de Lista de Membros da Equipa.
 *
 * Este objeto é utilizado para testar a renderização da lista de jogadores/staff e os seus filtros associados
 * em ambientes de desenvolvimento (Previews do Jetpack Compose) e testes unitários.
 */
object ListMembersMocks {
    /**
     * Lista de opções para o filtro de "Tipo de Membro" (ex: Jogador, Treinador, Admin).
     *
     * **Nota de UI:** A lista começa com `null`. Isto serve para representar a opção "Todos" ou "Indiferente"
     * num componente de seleção (Dropdown/Spinner), permitindo ao utilizador limpar o filtro.
     *
     * Contém: `[null, ADMIN_TEAM, PLAYER, ...]`
     */
    val mockListTypes = listOf(null) + TypeMember.values().toList()

    /**
     * Lista de opções para o filtro de "Posição em Campo".
     *
     * **Nota de UI:** Tal como `mockListTypes`, inicia com `null` para representar a opção "Todas as Posições".
     *
     * Contém: `[null, GOALKEEPER, DEFENDER, ...]`
     */
    val mockListPositions = listOf(null) + Position.values().toList()

    /**
     * Lista estática de membros da equipa com dados variados para testar diferentes estados visuais.
     *
     * **Cenários incluídos:**
     * 1. **Admin (João Silva):** `isAdmin = true`. Deve exibir ícones ou permissões de gestão.
     * 2. **Jogador Regular (Pedro Santos):** `isAdmin = false`, posição definida.
     * 3. **Jogador Alto (Miguel Costa):** Útil para testar layouts adaptativos ou filtros de características físicas.
     */
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

    /**
     * Implementação "No-Op" (No Operation) das ações de filtro da lista de membros.
     *
     * Define callbacks vazios `{}` para os eventos de mudança de filtros e botões de aplicar/limpar,
     * permitindo a interação segura com a UI de filtros em Previews.
     */
    val mockFilterActions =
        _root_ide_package_.com.example.amfootball.ui.actions.filters.FilterMemberTeamAction(
            {},
            {},
            {},
            {},
            {},
            _root_ide_package_.com.example.amfootball.ui.actions.filters.ButtonFilterActions({}, {})
        )

    /**
     * Implementação "No-Op" das ações individuais de cada membro na lista.
     *
     * Fornece callbacks vazios para operações de gestão (Promover, Despromover, Remover),
     * permitindo testar o layout dos cartões de membro e menus de contexto sem lógica de negócio real.
     */
    val mockItemActions =
        _root_ide_package_.com.example.amfootball.ui.actions.itemsList.ItemsListMemberAction(
            onPromoteMember = {},
            onDemoteMember = {},
            onRemovePlayer = {},
            onShowMoreInfo = { _, _ -> }
        )

}

/**
 * Fornece dados fictícios (Mocks) e stubs de ações para o ecrã de Listagem de Pedidos de Adiamento (Postpone Requests).
 *
 * Este objeto é utilizado para popular as pré-visualizações (Previews) e testes de UI, permitindo validar
 * a renderização dos cartões que comparam a data original do jogo com a nova data proposta pelo adversário.
 */
object ListPostPoneMatchMocks {
    /**
     * Implementação "No-Op" (No Operation) das ações de filtro.
     *
     * Define callbacks vazios `{}` para os critérios de filtragem, permitindo renderizar a
     * barra de filtros sem necessidade de lógica de backend.
     */
    val mockFilterActions =
        _root_ide_package_.com.example.amfootball.ui.actions.filters.FilterListPostPoneMatchActions(
            {},
            {},
            {},
            {},
            {},
            {},
            _root_ide_package_.com.example.amfootball.ui.actions.filters.ButtonFilterActions({}, {})
        )

    /**
     * Implementação "No-Op" das ações de decisão sobre os pedidos.
     *
     * Fornece callbacks vazios para Aceitar ou Rejeitar o adiamento, permitindo testar
     * a interatividade dos botões nos cartões de pedido.
     */
    val mockItemActions =
        _root_ide_package_.com.example.amfootball.ui.actions.itemsList.ItemsListPostPoneMatchActions(
            acceptPostPoneMatch = { _, _, _ -> },
            rejectPostPoneMatch = { _, _ -> },
            showMoreInfo = { _, _ -> }
        )

    /**
     * Data de referência estática (timestamp atual) utilizada para calcular as datas relativas
     * nos mocks abaixo. Garante que os testes têm uma base temporal consistente.
     */
    val mockDate = LocalDateTime.now()

    private val myTeamMock = TeamDto(
        id = "99",
        name = "Minha Equipa",
        image = ""
    )

    /**
     * Lista estática de pedidos de adiamento com cenários de teste.
     *
     * **Cenários incluídos:**
     * 1. **Adversário "Dragons FC":** Propõe adiar o jogo de amanhã (`plusDays(1)`) para depois de amanhã (`plusDays(2)`).
     * 2. **Adversário "Lions Club":** Propõe adiar um jogo da próxima semana (`plusDays(5)`) para dois dias depois (`plusDays(7)`).
     *
     * Estes dados permitem verificar se a UI formata corretamente a diferença de datas e locais.
     */
    val mockPostPoneMatches = listOf(
        PostponeDto(
            idMatch = "1",
            gameDateStr = mockDate.plusDays(1).toUiString(),
            postponeDateStr = mockDate.plusDays(2).toUiString(),
            team = myTeamMock,
            opponent = TeamDto(
                id = "10",
                name = "Dragons FC",
                image = ""
            )
        ),
        PostponeDto(
            idMatch = "2",
            gameDateStr = mockDate.plusDays(5).toUiString(),
            postponeDateStr = mockDate.plusDays(7).toUiString(),
            team = myTeamMock,
            opponent = TeamDto(
                id = "11",
                name = "Lions Club",
                image = ""
            )
        )
    )
}

object ItemActionsMock {
    val mockShowMoreItensAction = ShowMoreItensAction(
        isValidShowMore = { MutableStateFlow(true) },
        onLoadMore = {}
    )

    // Mock para quando NÃO há mais itens
    val mockShowMoreItensActionHidden = ShowMoreItensAction(
        isValidShowMore = { MutableStateFlow(false) },
        onLoadMore = {}
    )
}