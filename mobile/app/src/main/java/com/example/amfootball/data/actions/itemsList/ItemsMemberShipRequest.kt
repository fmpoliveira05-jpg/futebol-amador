package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

/**
 * Encapsula as ações de interação disponíveis para um item de "Pedido de Adesão" (Membership Request).
 *
 * Esta classe gere o fluxo de decisão sobre pedidos de entrada em equipas (ou convites de equipas a jogadores),
 * permitindo aceitar, rejeitar ou visualizar os detalhes de quem enviou o pedido.
 *
 * @property acceptMemberShipRequest Ação para aceitar o pedido de adesão.
 * @property rejectMemberShipRequest Ação para rejeitar o pedido de adesão.
 * @property showMore Ação para visualizar o perfil ou detalhes de quem enviou o pedido.
 */
data class ItemsMemberShipRequest (
    val acceptMemberShipRequest: (idReceiver: String, idRequest: String,
                                  isPlayerSender: Boolean, navHostController: NavHostController) -> Unit,
    val rejectMemberShipRequest: (idReceiver: String, idRequest: String, isPlayerSender: Boolean) -> Unit,
    val showMore: (idSender: String, isPlayerSender: Boolean, navHostController: NavHostController) -> Unit
)