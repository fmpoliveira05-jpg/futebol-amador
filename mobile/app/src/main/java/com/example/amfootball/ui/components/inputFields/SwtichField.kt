package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Check
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Switch
import androidx.compose.material3.SwitchDefaults
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

/**
 * Um componente Composable que encapsula o [Switch] do Material 3,
 * adicionando um rótulo de texto e ícones personalizados para os estados ON/OFF.
 *
 * O ícone no botão deslizante (thumb) muda para [Icons.Default.Check] quando ligado
 * e para [Icons.Default.Close] quando desligado.
 *
 * @param value O estado atual do Switch (true para Ligado/Checked, false para Desligado/Unchecked).
 * O valor padrão é true.
 * @param onCheckedChange Callback executado quando o estado do Switch é alterado pelo usuário.
 * Recebe o novo valor (Boolean).
 * @param text O rótulo de texto principal exibido ao lado do Switch.
 * @param textChecked A descrição de conteúdo (acessibilidade) usada quando o Switch está LIGADO (Check).
 * @param textUnChecked A descrição de conteúdo (acessibilidade) usada quando o Switch está DESLIGADO (Close).
 * @param enabled Define se o Switch está habilitado ou desabilitado.
 */
@Composable
fun Switcher(
    value: Boolean = true,
    onCheckedChange: (Boolean) -> Unit,
    text: String,
    textChecked: String,
    textUnChecked: String,
    enabled: Boolean = true
) {
    Column {
        Row(
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(
                text = text,
                color = MaterialTheme.colorScheme.onSurface
            )
            Spacer(modifier = Modifier.width(8.dp))

            Switch(
                checked = value,
                onCheckedChange = onCheckedChange,
                thumbContent = {
                    Icon(
                        imageVector = if (value) {
                            Icons.Default.Check
                        } else {
                            Icons.Default.Close
                        },
                        contentDescription = if (value) {
                            textChecked
                        } else {
                            textUnChecked
                        },
                        tint = if (value) {
                            MaterialTheme.colorScheme.onPrimaryContainer
                        } else {
                            MaterialTheme.colorScheme.onSurfaceVariant
                        }
                    )
                },
                enabled = enabled,
                colors = SwitchDefaults.colors(
                    //CHECKED
                    disabledCheckedThumbColor = MaterialTheme.colorScheme.onPrimary,
                    disabledCheckedTrackColor = MaterialTheme.colorScheme.primary,
                    disabledCheckedIconColor = MaterialTheme.colorScheme.onPrimaryContainer,
                    disabledCheckedBorderColor = Color.Transparent,

                    //UNCHECKED
                    disabledUncheckedThumbColor = MaterialTheme.colorScheme.outline,
                    disabledUncheckedTrackColor = MaterialTheme.colorScheme.surfaceContainerHighest,
                    disabledUncheckedBorderColor = MaterialTheme.colorScheme.outline,
                    disabledUncheckedIconColor = MaterialTheme.colorScheme.surfaceVariant
                )
            )
        }
    }
}