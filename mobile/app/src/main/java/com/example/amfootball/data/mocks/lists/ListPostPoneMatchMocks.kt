package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPostPoneMatchActions
import com.example.amfootball.data.actions.itemsList.ItemsListPostPoneMatchActions
import com.example.amfootball.data.dtos.match.PostPoneMatchDto
import com.example.amfootball.data.dtos.support.TeamDto
import java.time.LocalDateTime

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
    val mockFilterActions = FilterListPostPoneMatchActions(
        {}, {}, {}, {}, {}, {}, ButtonFilterActions({}, {})
    )

    /**
     * Implementação "No-Op" das ações de decisão sobre os pedidos.
     *
     * Fornece callbacks vazios para Aceitar ou Rejeitar o adiamento, permitindo testar
     * a interatividade dos botões nos cartões de pedido.
     */
    val mockItemActions = ItemsListPostPoneMatchActions(
        acceptPostPoneMatch = {},
        rejectPostPoneMatch = {},
        showMoreInfo = { _, _ -> }
    )

    /**
     * Data de referência estática (timestamp atual) utilizada para calcular as datas relativas
     * nos mocks abaixo. Garante que os testes têm uma base temporal consistente.
     */
    val mockDate = LocalDateTime.now()

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