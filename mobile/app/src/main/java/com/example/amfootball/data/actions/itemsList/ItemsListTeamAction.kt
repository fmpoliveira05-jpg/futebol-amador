package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

/**
 * Data class que agrupa as funções de callback (ações) que podem ser executadas
 * num item de equipa listado, tipicamente na tela de listagem de todas as equipas.
 *
 * Estas ações envolvem o início de fluxos de comunicação ou navegação para detalhes.
 *
 * @property onSendMatchInvite Callback para iniciar o fluxo de envio de convite de partida. Recebe o ID da equipa-alvo (String) e o [NavHostController].
 * @property onSendMemberShipRequest Callback para iniciar o fluxo de envio de pedido de adesão. Recebe o ID da equipa-alvo (String) e o [NavHostController].
 * @property onShowMore Callback para navegar para a tela de detalhes da equipa. Recebe o ID da equipa-alvo (String) e o [NavHostController].
 */
data class ItemsListTeamAction(
    val onSendMatchInvite: (idTeam: String, nameTeam: String, navHostController: NavHostController) -> Unit,
    val onSendMemberShipRequest: (idTeam: String, navHostController: NavHostController) -> Unit,
    val onShowMore: (idTeam: String, navHostController: NavHostController) -> Unit
)
