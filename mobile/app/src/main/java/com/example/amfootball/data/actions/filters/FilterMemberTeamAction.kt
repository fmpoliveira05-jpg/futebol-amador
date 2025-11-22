package com.example.amfootball.data.actions.filters

import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember

/**
 * Data class que agrupa todas as funções de callback (ações) necessárias para manipular
 * o estado de filtragem num ecrã de listagem de membros de uma equipa.
 *
 * @property onTypeMemberChange Callback para atualizar o filtro por tipo de membro ([TypeMember]: Jogador, Treinador, etc.).
 * @property onNameChange Callback para atualizar o filtro por nome do membro (String).
 * @property onMinAgeChange Callback para atualizar a idade mínima no filtro (Int?).
 * @property onMaxAgeChange Callback para atualizar a idade máxima no filtro (Int?).
 * @property onPositionChange Callback para atualizar o filtro por posição de jogo ([Position] ou null).
 * @property buttonActions Objeto que contém os callbacks para os botões de controlo (Aplicar e Limpar).
 */
data class FilterMemberTeamAction(
    val onTypeMemberChange: (TypeMember?) -> Unit,
    val onNameChange: (String) -> Unit,
    val onMinAgeChange: (Int?) -> Unit,
    val onMaxAgeChange: (Int?) -> Unit,
    val onPositionChange: (Position?) -> Unit,
    val buttonActions: ButtonFilterActions
)
