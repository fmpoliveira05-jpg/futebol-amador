package com.example.amfootball.data.actions.filters

/**
 * Data class que agrupa as ações lambda a serem executadas pelos botões de controlo de filtro
 * (Aplicar e Limpar).
 *
 * É utilizada para simplificar a passagem de múltiplos callbacks de ação para um componente Composable
 * (como a barra de botões de filtro).
 *
 * @property onFilterApply A função lambda (callback) a ser executada quando o botão "Aplicar Filtros" é pressionado.
 * @property onFilterClean A função lambda (callback) a ser executada quando o botão "Limpar Filtros" (Reset) é pressionado.
 */
data class ButtonFilterActions(
    val onFilterApply: () -> Unit,
    val onFilterClean: () -> Unit,
)