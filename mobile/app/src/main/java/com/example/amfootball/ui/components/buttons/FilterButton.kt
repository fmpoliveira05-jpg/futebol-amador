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

@Composable
fun FilterApplyButton(
    onClick: () -> Unit,
    text: String = stringResource(id = R.string.filter_button),
    contentDescription: String = stringResource(id = R.string.filter_button_description),
    modifier: Modifier = Modifier,
) {
    Button(
        onClick = onClick,
        modifier = modifier
    ) {
        Icon(
            imageVector = Icons.Default.FilterAlt,
            contentDescription = contentDescription // Acessibilidade
        )

        Spacer(Modifier.width(ButtonDefaults.IconSpacing)) // Espaço padrão

        Text(
            text = text,
            style = MaterialTheme.typography.labelLarge
        )
    }
}

@Composable
fun ClearFilterButton(onClick: () -> Unit,
                      text: String = stringResource(id = R.string.clear_button),
                      contentDescription: String = stringResource(id = R.string.clear_button_description),
                      modifier: Modifier = Modifier,
) {
    OutlinedButton(
        onClick = onClick,
        modifier = modifier,
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

@Composable
fun LineClearFilterButtons(
    buttonsActions: ButtonFilterActions,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = Modifier.fillMaxWidth(),
        horizontalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        FilterApplyButton(
            onClick = buttonsActions.onFilterApply,
            modifier = modifier
        )

        ClearFilterButton(
            onClick = buttonsActions.onFilterClean,
            modifier = modifier
        )
    }
}