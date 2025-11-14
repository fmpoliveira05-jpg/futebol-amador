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
import java.util.Calendar
import androidx.compose.material3.TimePicker
import androidx.compose.ui.res.stringResource
import java.util.Locale
import com.example.amfootball.R

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun FieldTimePicker(
    value: String,
    onValueChange: (String) -> Unit,
    label: String,
    contentDescription: String,
    isError: Boolean = false,
    errorMessage: String? = stringResource(id = R.string.mandatory_field),
) {
    var showTimePicker by remember { mutableStateOf(false) }

    OutlinedTextField(
        value = value,
        onValueChange = {},
        label = { Text(text = label) },
        readOnly = true,
        trailingIcon = {
            IconButton(onClick = { showTimePicker = true }) {
                Icon(
                    imageVector = Icons.Default.AccessTime,
                    contentDescription = contentDescription)
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
        modifier = Modifier.fillMaxWidth()
    )

    if (showTimePicker) {
        TimePickerApp(
            onDismiss = {
                showTimePicker = false
            },
            onConfirm = { timePickerState ->
                //Ao clicar vamos tranformar a string em data
                val hour = timePickerState.hour
                val minute = timePickerState.minute
                val newTimeValue = String.format(Locale.getDefault(), "%02d:%02d", hour, minute)

                //Vamos atualizar o valor
                onValueChange(newTimeValue)

                showTimePicker = false
            }
        )
    }
}

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
        },
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
            content = content
        )
    }
}

@Composable
private fun SurfaceTimePickerDialog(title: String,
                                    onDismiss: () -> Unit,
                                    onConfirm: () -> Unit,
                                    toggle: @Composable () -> Unit = {},
                                    content: @Composable () -> Unit) {
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