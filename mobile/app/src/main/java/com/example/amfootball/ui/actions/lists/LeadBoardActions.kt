package com.example.amfootball.ui.actions.lists

import androidx.navigation.NavHostController

/**
 * Data class que encapsula as ações de navegação e interatividade disponíveis
 * na tela de Tabela de Classificação (Leaderboard).
 * * Este padrão é comum no Compose para injetar todas as interações de uma tela
 * numa única variável para ser facilmente passada para componentes filhos.
 *
 * @property onShowMore A função lambda a ser invocada quando o utilizador deseja
 * ver mais detalhes sobre uma equipa específica.
 * Geralmente desencadeia uma navegação para o perfil da equipa.
 * @property showMoreItensAction Representa a lógica ou estado necessário para exibir
 * mais itens numa lista (e.g., paginação ou carregamento infinito).
 */
data class LeadBoardActions(
    val onShowMore: (idTeam: String, navHostController: NavHostController) -> Unit,
)