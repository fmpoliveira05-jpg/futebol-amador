package com.example.amfootball.data.mocks.lists

import com.example.amfootball.data.actions.filters.ButtonFilterActions
import com.example.amfootball.data.actions.filters.FilterListPlayersActions
import com.example.amfootball.data.dtos.player.InfoPlayerDto
import com.example.amfootball.data.enums.Position

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
    val Actions = FilterListPlayersActions(
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