package com.example.amfootball.ui.components.lists

import androidx.compose.animation.AnimatedVisibility
import androidx.compose.animation.ExperimentalAnimationApi
import androidx.compose.animation.expandVertically
import androidx.compose.animation.shrinkVertically
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.RowScope
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.ExpandLess
import androidx.compose.material.icons.filled.ExpandMore
import androidx.compose.material3.ElevatedCard
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

/**
 * Componente Composable que cria uma secção de filtro colapsável (Accordion-style).
 *
 * Utiliza um [ElevatedCard] como container e [AnimatedVisibility] para exibir ou ocultar
 * o conteúdo com transições verticais suaves.
 *
 * @param isExpanded Booleano que define se a secção de conteúdo está visível.
 * @param onToggleExpand Callback chamado quando o cabeçalho é clicado para alternar o estado de expansão.
 * @param modifier Modificador para estilizar o [ElevatedCard].
 * @param header O slot Composable para o cabeçalho da secção. Por padrão, usa [FilterHeader].
 * @param content O slot Composable que contém os elementos de filtro. É passado um [Modifier]
 * com preenchimento (padding) aplicado.
 */
@OptIn(ExperimentalAnimationApi::class)
@Composable
fun FilterSection(
    isExpanded: Boolean,
    onToggleExpand: () -> Unit,
    modifier: Modifier = Modifier,
    header: @Composable () -> Unit = {
        FilterHeader(isExpanded = isExpanded, onToggleExpand = onToggleExpand)
    },
    content: @Composable (modifier: Modifier) -> Unit = {}
) {
    ElevatedCard(modifier = modifier.fillMaxWidth()) {
        Column {
            header()

            AnimatedVisibility(
                visible = isExpanded,
                enter = expandVertically(),
                exit = shrinkVertically(),
            ) {
                content(Modifier.padding(start = 16.dp, end = 16.dp, bottom = 16.dp))
            }
        }
    }
}

/**
 * O cabeçalho padrão para o [FilterSection].
 *
 * Exibe o título ("Filtros") e um ícone que reflete o estado atual (seta para cima/baixo).
 * Todo o Row é clicável para alternar o estado de expansão.
 *
 * @param isExpanded Booleano que indica se a secção de conteúdo está atualmente aberta.
 * @param onToggleExpand Callback para acionar a expansão/colapso.
 */
@Composable
fun FilterHeader(
    isExpanded: Boolean,
    onToggleExpand: () -> Unit,
) {
    Row(
        modifier = Modifier
            .fillMaxWidth()
            .clickable { onToggleExpand() }
            .padding(horizontal = 16.dp, vertical = 12.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = stringResource(id = R.string.title_filter_header),
            style = MaterialTheme.typography.titleMedium,
            modifier = Modifier.weight(1f)
        )
        Icon(
            imageVector = if (isExpanded) Icons.Default.ExpandLess else Icons.Default.ExpandMore,
            contentDescription = if (isExpanded) {
                stringResource(id = R.string.hide_filter)
            }  else {
                stringResource(id = R.string.open_filters)
            }
        )
    }
}

/**
 * Um componente de layout Composable que exibe os elementos de filtro numa única linha horizontal.
 *
 * Utiliza [Row] e [Arrangement.spacedBy] para adicionar espaçamento entre os itens de filtro.
 *
 * @param modifier Modificador para estilizar o [Row] (padrão: fillMaxWidth).
 * @param content Um [RowScope] que aceita múltiplos Composable, permitindo que os filtros
 * sejam dispostos lado a lado.
 * @param horizontalSpacing O espaçamento horizontal a ser aplicado entre cada item (padrão: 8.dp).
 */
@Composable
fun FilterRow(
    modifier: Modifier = Modifier.fillMaxWidth(),
    content: @Composable RowScope.() -> Unit,
    horizontalSpacing: Dp = 8.dp,
) {
    Row(modifier = modifier,
        horizontalArrangement = Arrangement.spacedBy(horizontalSpacing),
        verticalAlignment = Alignment.CenterVertically
    ) {
        content()
    }
}