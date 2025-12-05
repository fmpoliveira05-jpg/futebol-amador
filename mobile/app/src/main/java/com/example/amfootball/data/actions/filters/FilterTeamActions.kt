package com.example.amfootball.data.actions.filters

/**
 * Data class que agrupa todas as funções de callback (ações) necessárias para manipular
 * o estado de filtragem num ecrã de listagem de equipas.
 *
 * @property onNameChange Callback para atualizar o filtro por nome da equipa (String).
 * @property onCityChange Callback para atualizar o filtro por cidade (String).
 * @property onRankChange Callback para atualizar o filtro por posição/rank na tabela de classificação (String).
 * @property onMinPointChange Callback para atualizar a pontuação mínima no filtro (Int?).
 * @property onMaxPointChange Callback para atualizar a pontuação máxima no filtro (Int?).
 * @property onMinAgeChange Callback para atualizar a idade média mínima da equipa no filtro (Int?).
 * @property onMaxAgeChange Callback para atualizar a idade média máxima da equipa no filtro (Int?).
 * @property onMinNumberMembersChange Callback para atualizar o número mínimo de membros no filtro (Int?).
 * @property onMaxNumberMembersChange Callback para atualizar o número máximo de membros no filtro (Int?).
 * @property buttonActions Objeto que contém os callbacks para os botões de controlo (Aplicar e Limpar).
 */
data class FilterTeamActions(
    val onNameChange: (String) -> Unit,
    val onCityChange: (String) -> Unit,
    val onRankChange: (String) -> Unit,
    val onMinPointChange: (Int?) -> Unit,
    val onMaxPointChange: (Int?) -> Unit,
    val onMinAgeChange: (Int?) -> Unit,
    val onMaxAgeChange: (Int?) -> Unit,
    val onMinNumberMembersChange: (Int?) -> Unit,
    val onMaxNumberMembersChange: (Int?) -> Unit,
    val buttonActions: ButtonFilterActions
)