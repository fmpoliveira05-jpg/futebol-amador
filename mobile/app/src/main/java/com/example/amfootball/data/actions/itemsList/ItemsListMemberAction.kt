package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

/**
 * Data class que agrupa todas as ações (callbacks) disponíveis para um item individual na lista de membros.
 *
 * Esta estrutura é utilizada para passar funções de manipulação de estado do ViewModel para os
 * componentes UI (Composables), mantendo o desacoplamento. Permite gerir hierarquias e
 * permanência de membros na equipa.
 *
 * @property onPromoteMember Callback para elevar o nível de acesso de um membro (ex: de Membro para Admin).
 * Recebe o [idPlayer] do membro a ser promovido.
 * @property onDemoteMember Callback para reduzir o nível de acesso de um membro (ex: de Admin para Membro).
 * Recebe o [idAdmin] do administrador a ser despromovido.
 * @property onRemovePlayer Callback para remover (expulsar) um jogador da equipa.
 * Recebe o [idPlayer] do jogador a ser removido.
 * @property onShowMoreInfo Callback para navegar para o ecrã de detalhes/perfil do jogador.
 * Recebe o [playerId] e o [NavHostController] necessário para a navegação.
 */
data class ItemsListMemberAction(
    val onPromoteMember: (idPlayer: String) -> Unit,
    val onDemoteMember: (idAdmin: String) -> Unit,
    val onRemovePlayer: (idPlayer: String) -> Unit,
    val onShowMoreInfo: (playerId: String, navHostController: NavHostController) -> Unit,
)
