package com.example.amfootball.data.actions.lists

import androidx.navigation.NavHostController
import androidx.compose.runtime.State

/**
 * Encapsula as ações de interação e controlo de paginação para o ecrã de Classificações (Leaderboard).
 *
 * Esta classe gere a navegação para os detalhes das equipas listadas e a lógica de carregamento incremental
 * (paginação/infinite scroll) da tabela.
 *
 * @property onShowMore Ação para visualizar os detalhes de uma equipa presente na tabela.
 * @property isValidShowMoreTeams Função que verifica se existem mais equipas disponíveis para carregar na lista.
 * Retorna um [State] de `Boolean` (observável pelo Jetpack Compose) que deve ser usado para controlar
 * a visibilidade ou o estado "enabled" do botão/trigger de paginação.
 *
 * @property onLoadMoreTeams Ação disparada para solicitar o carregamento da próxima página de equipas.
 * Geralmente invocada quando o utilizador chega ao fim da lista atual ou clica num botão de "Carregar Mais".
 */
data class LeadBoardActions(
    val onShowMore: (idTeam: String, navHostController: NavHostController) -> Unit,
    val isValidShowMoreTeams: () -> State<Boolean>,
    val onLoadMoreTeams: () -> Unit
)