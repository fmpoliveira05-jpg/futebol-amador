package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

/**
 * Encapsula as ações de interação disponíveis para um item de "Pedido de Adiamento de Jogo" numa lista.
 *
 * Esta classe gere as decisões do utilizador relativamente a pedidos de remarcação/adiamento (postpone),
 * permitindo aceitar ou rejeitar o pedido, bem como consultar detalhes do adversário.
 *
 * @property acceptPostPoneMatch Ação para aceitar o pedido de adiamento do jogo.
 * @property rejectPostPoneMatch Ação para rejeitar o pedido de adiamento do jogo.
 * @property showMoreInfo Ação para visualizar mais detalhes sobre a equipa adversária envolvida no pedido.
 */
data class ItemsListPostPoneMatchActions(
    val acceptPostPoneMatch: (idPostPoneMatch: String) -> Unit,
    val rejectPostPoneMatch: (idPostPoneMatch: String) -> Unit,
    val showMoreInfo: (idOpponent: String, navHostController: NavHostController) -> Unit
)
