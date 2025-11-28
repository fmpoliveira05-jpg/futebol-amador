package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.IntrinsicSize
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.AccessTime
import androidx.compose.material.icons.filled.EditCalendar
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.TimeInput
import androidx.compose.material3.TimePickerState
import androidx.compose.material3.rememberTimePickerState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Dialog
import androidx.compose.ui.window.DialogProperties
import androidx.compose.material3.TextFieldDefaults
import java.util.Calendar
import androidx.compose.material3.TimePicker
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.stringResource
import java.util.Locale
import com.example.amfootball.R

/**
 * Componente Composable que exibe um campo de texto de leitura, que, ao ser clicado,
 * abre o diálogo customizado [TimePickerApp] para selecionar um horário.
 *
 * @param value A string atual do horário selecionado (ex: "14:30").
 * @param onValueChange Callback chamado quando um novo horário é confirmado, retornando a string formatada ("HH:mm").
 * @param label O rótulo a ser exibido no campo de texto.
 * @param contentDescription Descrição para acessibilidade do ícone do relógio.
 * @param isError Indica se o campo deve mostrar o estado de erro.
 * @param errorMessage Mensagem de erro a ser exibida se [isError] for true.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun FieldTimePicker(
    value: String,
    onValueChange: (String) -> Unit,
    label: String,
    contentDescription: String,
    isError: Boolean = false,
    errorMessage: String? = stringResource(id = R.string.mandatory_field),
    enabled: Boolean = true
) {
    var showTimePicker by remember { mutableStateOf(false) }

    OutlinedTextField(
        value = value,
        onValueChange = {},
        label = { Text(text = label) },
        trailingIcon = {
            IconButton(
                onClick = { if (enabled) showTimePicker = true },
                enabled = enabled
            ) {
                Icon(
                    imageVector = Icons.Default.AccessTime,
                    contentDescription = contentDescription,
                    tint = if (enabled) {
                        MaterialTheme.colorScheme.onSurfaceVariant
                    }
                    else {
                        MaterialTheme.colorScheme.onSurfaceVariant
                    }
                )
            }
        },
        supportingText = {
            if (isError && errorMessage != null) {
                Text(
                    text = errorMessage,
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodySmall
                )
            }
        },
        readOnly = true,
        enabled = enabled,
        modifier = Modifier.fillMaxWidth(),
        colors = TextFieldDefaults.colors(
            // --- TEXTO ---
            focusedTextColor = MaterialTheme.colorScheme.onSurface,
            unfocusedTextColor = MaterialTheme.colorScheme.onSurface,
            disabledTextColor = MaterialTheme.colorScheme.onSurface,

            // --- LABEL ---
            focusedLabelColor = MaterialTheme.colorScheme.primary,
            unfocusedLabelColor = MaterialTheme.colorScheme.onSurfaceVariant,
            disabledLabelColor = MaterialTheme.colorScheme.onSurfaceVariant,

            // --- BORDAS (OUTLINE) ---
            focusedIndicatorColor = MaterialTheme.colorScheme.primary,
            unfocusedIndicatorColor = MaterialTheme.colorScheme.outline,
            disabledIndicatorColor = MaterialTheme.colorScheme.outline,

            // --- CONTAINER (FUNDO) ---
            focusedContainerColor = Color.Transparent,
            unfocusedContainerColor = Color.Transparent,
            disabledContainerColor = Color.Transparent,

            // --- ÍCONES (Leading / Trailing) ---
            focusedTrailingIconColor = MaterialTheme.colorScheme.onSurfaceVariant,
            unfocusedTrailingIconColor = MaterialTheme.colorScheme.onSurfaceVariant,
            disabledTrailingIconColor = MaterialTheme.colorScheme.onSurfaceVariant,
            disabledLeadingIconColor = MaterialTheme.colorScheme.onSurfaceVariant,

            // --- SUPPORTING TEXT (Mensagens de erro/ajuda) ---
            focusedSupportingTextColor = MaterialTheme.colorScheme.onSurfaceVariant,
            unfocusedSupportingTextColor = MaterialTheme.colorScheme.onSurfaceVariant,
            disabledSupportingTextColor = MaterialTheme.colorScheme.onSurfaceVariant,

            // --- PLACEHOLDER ---
            disabledPlaceholderColor = MaterialTheme.colorScheme.onSurfaceVariant
        )
    )

    if (showTimePicker && enabled) {
        TimePickerApp(
            onDismiss = {
                showTimePicker = false
            },
            onConfirm = { timePickerState ->
                val hour = timePickerState.hour
                val minute = timePickerState.minute
                val newTimeValue = String.format(Locale.getDefault(), "%02d:%02d", hour, minute)
                onValueChange(newTimeValue)
                showTimePicker = false
            },
        )
    }
}

/**
 * Componente privado que contém o estado do seletor de tempo e a lógica de alternância
 * entre os modos [TimePicker] (Relógio) e [TimeInput] (Teclado).
 *
 * Utiliza o [TimePickerDialog] para envolver o seletor numa caixa de diálogo.
 *
 * @param onConfirm Callback executado ao pressionar "Ok". Recebe o [TimePickerState] final.
 * @param onDismiss Callback executado ao pressionar "Cancel" ou fora do diálogo.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun TimePickerApp(
    onConfirm: (TimePickerState) -> Unit,
    onDismiss: () -> Unit,
)  {
    val currentTime = Calendar.getInstance()

    val timePickerState = rememberTimePickerState(
        initialHour = currentTime.get(Calendar.HOUR_OF_DAY),
        initialMinute = currentTime.get(Calendar.MINUTE),
        is24Hour = true
    )

    /** Determines whether the time picker is dial or input */

    var showDial by remember { mutableStateOf(true) }

    val toggleIcon = if (showDial) {
        Icons.Filled.EditCalendar
    } else {
        Icons.Filled.AccessTime
    }

    TimePickerDialog(
        title = "Select Time",
        onDismiss = { onDismiss() },
        onConfirm = { onConfirm(timePickerState) },
        toggle = {
            IconButton(onClick = { showDial = !showDial }) {
                Icon(
                    imageVector = toggleIcon,
                    contentDescription = "Time picker type toggle",
                )
            }
        }
    ) {
        if (showDial) {
            TimePicker(
                state = timePickerState,
            )
        } else {
            TimeInput(
                state = timePickerState,
            )
        }
    }
}

/**
 * Componente privado que cria a estrutura básica do [Dialog] para o seletor de tempo.
 *
 * Garante que o diálogo utilize o [SurfaceTimePickerDialog] para estilização Material 3.
 *
 * @param title O título exibido no cabeçalho do diálogo.
 * @param onDismiss Ação para fechar o diálogo.
 * @param onConfirm Ação executada ao confirmar a seleção.
 * @param toggle Slot Composable para o ícone de alternância de modo (Dial/Input).
 * @param content O conteúdo principal do diálogo (o [TimePicker] ou [TimeInput]).
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun TimePickerDialog(
    title: String,
    onDismiss: () -> Unit,
    onConfirm: () -> Unit,
    toggle: @Composable () -> Unit = {},
    content: @Composable () -> Unit,
) {
    Dialog(
        onDismissRequest = onDismiss,
        properties = DialogProperties(usePlatformDefaultWidth = false),
    ) {
        SurfaceTimePickerDialog(
            title = title,
            onDismiss = onDismiss,
            onConfirm = onConfirm,
            toggle = toggle,
            content = content,
        )
    }
}

/**
 * Componente privado que define a aparência e a estrutura de botões (Ok/Cancel) e título do diálogo.
 *
 * Utiliza [Surface] para aplicar a forma arredondada do Material 3 e tonalElevation.
 *
 * @param title O título a ser exibido no cabeçalho.
 * @param onDismiss Ação executada ao clicar em "Cancel".
 * @param onConfirm Ação executada ao clicar em "Ok".
 * @param toggle Slot Composable para o ícone de alternância (Dial/Input).
 * @param content O conteúdo principal do diálogo (o seletor de tempo).
 */
@Composable
private fun SurfaceTimePickerDialog(
    title: String,
    onDismiss: () -> Unit,
    onConfirm: () -> Unit,
    toggle: @Composable () -> Unit = {},
    content: @Composable () -> Unit
) {
    Surface(
        shape = MaterialTheme.shapes.extraLarge,
        tonalElevation = 6.dp,
        modifier = Modifier
            .width(IntrinsicSize.Min)
            .height(IntrinsicSize.Max)
            .background(
                shape = MaterialTheme.shapes.extraLarge,
                color = MaterialTheme.colorScheme.surface
            ),
    ) {
        Column(
            modifier = Modifier.padding(24.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(bottom = 20.dp),
                text = title,
                style = MaterialTheme.typography.labelMedium
            )
            content()
            Row(
                modifier = Modifier
                    .height(40.dp)
                    .fillMaxWidth()
            ) {
                toggle()
                Spacer(modifier = Modifier.weight(1f))
                TextButton(onClick = onDismiss) { Text(text ="Cancel")}
                TextButton(onClick = onConfirm) { Text(text = "Ok")}
            }
        }
    }
}