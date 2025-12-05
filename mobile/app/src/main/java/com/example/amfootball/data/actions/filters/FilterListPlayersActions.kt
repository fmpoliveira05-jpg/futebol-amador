package com.example.amfootball.data.actions.filters

import com.example.amfootball.data.enums.Position

/**
 * Data class que agrupa todas as funções de callback (ações) necessárias para manipular
 * o estado de filtragem num ecrã de listagem de jogadores.
 *
 * É o objeto principal de dependência passado da ViewModel para o ecrã de UI do filtro de jogadores.
 *
 * @property onNameChange Callback para atualizar o filtro por nome do jogador (String).
 * @property onCityChange Callback para atualizar o filtro por cidade (String).
 * @property onMinAgeChange Callback para atualizar a idade mínima no filtro (Int?).
 * @property onMaxAgeChange Callback para atualizar a idade máxima no filtro (Int?).
 * @property onPositionChange Callback para atualizar o filtro por posição de jogo ([Position] ou null).
 * @property onMinSizeChange Callback para atualizar a altura mínima no filtro (em Int? centímetros).
 * @property onMaxSizeChange Callback para atualizar a altura máxima no filtro (em Int? centímetros).
 * @property buttonActions Objeto que contém os callbacks para os botões de controlo (Aplicar e Limpar).
 */
data class FilterListPlayersActions(
    val onNameChange: (String) -> Unit,
    val onCityChange: (String) -> Unit,
    val onMinAgeChange: (Int?) -> Unit,
    val onMaxAgeChange: (Int?) -> Unit,
    val onPositionChange: (Position?) -> Unit,
    val onMinSizeChange: (Int?) -> Unit,
    val onMaxSizeChange: (Int?) -> Unit,
    val buttonActions: ButtonFilterActions
)