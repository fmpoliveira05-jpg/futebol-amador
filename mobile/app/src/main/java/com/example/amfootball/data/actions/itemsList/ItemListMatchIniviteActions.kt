package com.example.amfootball.data.actions.itemsList

import androidx.navigation.NavHostController

/**
 * Encapsula as ações de interação disponíveis para um item individual de "Convite de Jogo" numa lista.
 *
 * Esta classe define os callbacks para responder às decisões do utilizador sobre um convite específico,
 * incluindo aceitar, rejeitar ou navegar para fluxos mais complexos como negociação e detalhes.
 *
 * @property acceptMatchInvite Ação para aceitar o convite de jogo.
 * @property rejectMatchInvite Ação para rejeitar o convite de jogo.
 * @property negociateMatchInvite Ação para iniciar o processo de negociação de um convite.
 * @property showMoreDetails Ação para visualizar os detalhes completos do convite.
 */
data class ItemListMatchIniviteActions(
    val acceptMatchInvite: (idMatchInvite: String) -> Unit,
    val rejectMatchInvite: (idMatchInvite: String) -> Unit,
    val negociateMatchInvite: (idMatchInvite: String, navHostController: NavHostController) -> Unit,
    val showMoreDetails: (idMatchInvite: String, navHostController: NavHostController) -> Unit
)