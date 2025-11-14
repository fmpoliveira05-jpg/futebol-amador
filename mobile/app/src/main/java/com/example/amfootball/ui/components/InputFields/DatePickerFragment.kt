package com.example.amfootball.ui.components.InputFields

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.heightIn
import androidx.compose.foundation.layout.offset
import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.DateRange
import androidx.compose.material3.DatePicker
import androidx.compose.material3.DatePickerDialog
import androidx.compose.material3.DatePickerState
import androidx.compose.material3.DisplayMode
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.SelectableDates
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.material3.rememberDatePickerState
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.shadow
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import androidx.compose.ui.window.Popup
import java.text.SimpleDateFormat
import java.util.Calendar
import java.util.Date
import java.util.Locale
import com.example.amfootball.R

/**
 * Docker
 * */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DatePickerDockedLimitedDate(value: String,
                                onDateSelected: (millis: Long) -> Unit,
                                label: String,
                                contentDescription: String,
                                isSingleLine: Boolean = false,
                                isError: Boolean = false,
                                errorMessage: String? = stringResource(id = R.string.mandatory_field),
                                modifier: Modifier = Modifier
) {
    var showDatePicker by remember { mutableStateOf(value = false) }

    val todayMillis = remember {
        Calendar.getInstance().apply {
            set(Calendar.HOUR_OF_DAY, 0)
            set(Calendar.MINUTE, 0)
            set(Calendar.SECOND, 0)
            set(Calendar.MILLISECOND, 0)
        }.timeInMillis
    }

    // Só permite datas que sejam >= hoje
    val selectableDates = object : SelectableDates {
        override fun isSelectableDate(utcTimeMillis: Long): Boolean {
            return utcTimeMillis >= todayMillis
        }
    }

    val datePickerState = rememberDatePickerState(
        selectableDates = selectableDates
    )

    DatePicker(
        label = label,
        value = value,
        onIconClick = { showDatePicker = !showDatePicker },
        contentDescription = contentDescription,
        showDatePicker = showDatePicker,
        datePickerState = datePickerState,
        onDismiss = { showDatePicker = false },
        onDateSelected = onDateSelected,
        isSingleLine = isSingleLine,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

@Composable
fun DatePickerDocked(value: String,
                     onDateSelected: (millis: Long) -> Unit,
                     label: String,
                     modifier: Modifier = Modifier,
                     contentDescription: String,
                     isError: Boolean = false,
                     errorMessage: String? = null
) {
    var showDatePicker by remember { mutableStateOf(value = false) }
    val datePickerState = rememberDatePickerState()

    DatePicker(
        label = label,
        value = value,
        onIconClick = { showDatePicker = !showDatePicker },
        contentDescription = contentDescription,
        showDatePicker = showDatePicker,
        datePickerState = datePickerState,
        onDismiss = { showDatePicker = false },
        onDateSelected = onDateSelected,
        isError = isError,
        errorMessage = errorMessage,
        modifier = modifier
    )
}

/**
 * Modal
 * */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DatePickerModal(
    onDateSelected: (Long?) -> Unit,
    onDismiss: () -> Unit
) {
    val datePickerState = rememberDatePickerState()

    DatePickerDialog(
        onDismissRequest = onDismiss,
        confirmButton = {
            TextButton(onClick = {
                onDateSelected(datePickerState.selectedDateMillis)
                onDismiss()
            }) {
                Text("OK")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    ) {
        DatePicker(state = datePickerState)
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun DatePickerModalInput(
    onDateSelected: (Long?) -> Unit,
    onDismiss: () -> Unit
) {
    val datePickerState = rememberDatePickerState(initialDisplayMode = DisplayMode.Input)

    DatePickerDialog(
        onDismissRequest = onDismiss,
        confirmButton = {
            TextButton(onClick = {
                onDateSelected(datePickerState.selectedDateMillis)
                onDismiss()
            }) {
                Text("OK")
            }
        },
        dismissButton = {
            TextButton(onClick = onDismiss) {
                Text("Cancel")
            }
        }
    ) {
        DatePicker(state = datePickerState)
    }
}

fun convertMillisToDate(millis: Long): String {
    val formatter = SimpleDateFormat("dd/MM/yyyy", Locale.getDefault())
    return formatter.format(Date(millis))
}

@Composable
private fun DatePicker(label: String,
                       value: String,
                       onIconClick: () -> Unit,
                       contentDescription: String,
                       showDatePicker: Boolean,
                       datePickerState: DatePickerState,
                       onDismiss: () -> Unit,
                       onDateSelected: (millis: Long) -> Unit,
                       isSingleLine: Boolean = false,
                       isError: Boolean = false,
                       errorMessage: String? = null,
                       modifier: Modifier = Modifier){

    LaunchedEffectDatePicker(
        datePickerState = datePickerState,
        onDateSelected = onDateSelected,
        onDismiss = onDismiss
    )

    Box(
        modifier = modifier
    ) {
        DateOutlineOutlinedTextField(
            value = value,
            label = label,
            contentDescription = contentDescription,
            onIconClick = onIconClick,
            isSingleLine = isSingleLine,
            isError = isError,
            errorMessage = errorMessage
        )

        if (showDatePicker) {
            PopUpDatePicker(
                datePickerState = datePickerState,
                onDismiss = onDismiss
            )
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun LaunchedEffectDatePicker(
    datePickerState: DatePickerState,
    onDateSelected: (millis: Long) -> Unit,
    onDismiss: () -> Unit
) {
    LaunchedEffect(datePickerState.selectedDateMillis) {
        datePickerState.selectedDateMillis?.let { millis ->
            onDateSelected(millis)
            onDismiss()
        }
    }
}

@Composable
private fun DateOutlineOutlinedTextField(value: String,
                                         label: String,
                                         contentDescription: String,
                                         onIconClick: () -> Unit,
                                         isSingleLine: Boolean = false,
                                         isError: Boolean = false,
                                         errorMessage: String? = null,
) {
    OutlinedTextField(
        value = value,
        onValueChange = { },
        label = { Text(text = label) },
        readOnly = true,
        trailingIcon = {
            IconButton(
                onClick = onIconClick
            ) {
                Icon(
                    imageVector = Icons.Default.DateRange,
                    contentDescription = contentDescription
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
        singleLine = isSingleLine,
        modifier = Modifier
            .fillMaxWidth()
            .heightIn(min = 64.dp)
    )
}

@Composable
private fun PopUpDatePicker(
    onDismiss: () -> Unit,
    datePickerState: DatePickerState
) {
    Popup(
        onDismissRequest = onDismiss,
        alignment = Alignment.TopStart
    ) {
        Box(
            modifier = Modifier
                .fillMaxWidth()
                .offset(y = 64.dp)
                .shadow(elevation = 4.dp)
                .background(color = MaterialTheme.colorScheme.surface)
                .padding(all = 16.dp)
        ) {
            DatePicker(
                state = datePickerState,
                showModeToggle = true,
            )
        }
    }
}