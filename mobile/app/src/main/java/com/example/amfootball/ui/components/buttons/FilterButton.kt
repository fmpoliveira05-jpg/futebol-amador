package com.example.amfootball.ui.components.buttons

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Clear
import androidx.compose.material.icons.filled.FilterAlt
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R
import com.example.amfootball.data.actions.filters.ButtonFilterActions

/**
 * Agrupa os botões de Limpeza de Filtros lado a lado.
 *
 * Utiliza um [Row] para alinhar [FilterApplyButton] e [ClearFilterButton] horizontalmente,
 * ocupando toda a largura disponível e espaçados por 8.dp.
 *
 * @param buttonsActions Um objeto contendo as lambdas de ação ([onFilterApply] e [onFilterClean])
 * a serem executadas quando os botões são clicados.
 * @param modifier Um [Modifier] opcional para configurar o layout da linha.
 */
@Composable
fun LineClearFilterButtons(
    buttonsActions: ButtonFilterActions,
    modifier: Modifier = Modifier,
    modifierFilter: Modifier = Modifier,
    modifierClear: Modifier = Modifier,
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        FilterApplyButton(
            onClick = buttonsActions.onFilterApply,
            modifier = modifier,
            modifierFilter = modifierFilter
        )

        ClearFilterButton(
            onClick = buttonsActions.onFilterClean,
            modifier = modifier,
            modifierClear = modifierClear
        )
    }
}

/**
 * Um botão principal ([Button] preenchido) usado para aplicar filtros.
 *
 * Exibe o ícone [Icons.Default.FilterAlt] e um texto padrão configurável.
 *
 * @param onClick A função lambda a ser executada quando o botão for clicado (ação de aplicar filtro).
 * @param text A string de texto a ser exibida no botão (padrão: [R.string.filter_button]).
 * @param contentDescription A descrição de conteúdo para acessibilidade do ícone (padrão: [R.string.filter_button_description]).
 * @param modifier Um [Modifier] opcional para configurar o botão.
 */
@Composable
fun FilterApplyButton(
    onClick: () -> Unit,
    text: String = stringResource(id = R.string.filter_button),
    contentDescription: String = stringResource(id = R.string.filter_button_description),
    modifier: Modifier = Modifier,
    modifierFilter: Modifier = Modifier
) {
    Button(
        onClick = onClick,
        modifier = modifier.then(modifierFilter)
    ) {
        Icon(
            imageVector = Icons.Default.FilterAlt,
            contentDescription = contentDescription
        )

        Spacer(Modifier.width(ButtonDefaults.IconSpacing))

        Text(
            text = text,
            style = MaterialTheme.typography.labelLarge
        )
    }
}

/**
 * Um botão secundário ([OutlinedButton]) usado para limpar ou resetar filtros.
 *
 * Exibe o ícone [Icons.Default.Clear] e um texto padrão configurável.
 *
 * @param onClick A função lambda a ser executada quando o botão for clicado (ação de limpar filtro).
 * @param text A string de texto a ser exibida no botão (padrão: [R.string.clear_button]).
 * @param contentDescription A descrição de conteúdo para acessibilidade do ícone (padrão: [R.string.clear_button_description]).
 * @param modifier Um [Modifier] opcional para configurar o botão.
 */
@Composable
fun ClearFilterButton(
    onClick: () -> Unit,
    text: String = stringResource(id = R.string.clear_button),
    contentDescription: String = stringResource(id = R.string.clear_button_description),
    modifier: Modifier = Modifier,
    modifierClear: Modifier = Modifier
) {
    OutlinedButton(
        onClick = onClick,
        modifier = modifier.then(modifierClear),
    ) {
        Icon(
            imageVector = Icons.Default.Clear,
            contentDescription = contentDescription
        )

        Spacer(Modifier.width(ButtonDefaults.IconSpacing))

        Text(
            text = text,
            style = MaterialTheme.typography.labelLarge
        )
    }
}