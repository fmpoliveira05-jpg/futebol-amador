package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterMemberTeamAction
import com.example.amfootball.data.actions.itemsList.ItemsListMemberAction
import com.example.amfootball.data.dtos.player.MemberTeamDto
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

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
    val mockFilterActions = FilterMemberTeamAction(
        {}, {}, {}, {}, {}, ButtonFilterActions({}, {})
    )

    /**
     * Implementação "No-Op" das ações individuais de cada membro na lista.
     *
     * Fornece callbacks vazios para operações de gestão (Promover, Despromover, Remover),
     * permitindo testar o layout dos cartões de membro e menus de contexto sem lógica de negócio real.
     */
    val mockItemActions = ItemsListMemberAction(
        onPromoteMember = {},
        onDemoteMember = {},
        onRemovePlayer = {},
        onShowMoreInfo = { _, _ -> }
    )

}