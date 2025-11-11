package com.example.amfootball.ui.components.Buttons

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.FilterAlt
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R

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