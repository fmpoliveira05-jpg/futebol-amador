package com.example.amfootball.ui.components.InputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.text.KeyboardOptions
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import com.example.amfootball.R

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun LabelTextField(
    label: String,
    value: String?,
    modifier: Modifier = Modifier,
    isSingleLine: Boolean = true,
    onValueChange: (String) -> Unit = {},
    isReadOnly: Boolean = false,
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String = stringResource(id = R.string.mandatory_field),
    keyboardOptions: KeyboardOptions = KeyboardOptions.Default,
) {
    Column(
        modifier = modifier
    ) {
        Label(label = label, isRequired = isRequired)

        TextFieldOutline(
            label = label,
            value = value,
            isSingleLine = isSingleLine,
            onValueChange = onValueChange,
            isReadOnly = isReadOnly,
            isError = isError,
            errorMessage = errorMessage,
            keyboardOptions = keyboardOptions,
        )
    }
}


@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TextFieldOutline(
    label: String,
    value: String?,
    modifier: Modifier = Modifier,
    isSingleLine: Boolean = true,
    onValueChange: (String) -> Unit = {},
    isReadOnly: Boolean = false,
    isRequired: Boolean = false,
    isError: Boolean = false,
    errorMessage: String = stringResource(id = R.string.mandatory_field),
    keyboardOptions: KeyboardOptions = KeyboardOptions.Default,
) {
    val labelText = formatRequiredLabel(label = label, isRequired = isRequired)

    Column(modifier = modifier) {
        OutlinedTextField(
            value = value ?: "",
            onValueChange = onValueChange,
            singleLine = isSingleLine,
            readOnly = isReadOnly,
            isError = isError,
            label = { Text(text = labelText) },
            placeholder = { Text(text = labelText) },
            modifier = Modifier.fillMaxWidth(),
            keyboardOptions = keyboardOptions,
            supportingText = {
                if (isError) {
                    Text(
                        text = errorMessage,
                        color = MaterialTheme.colorScheme.error
                    )
                }
            }
        )
    }
}
