package com.example.amfootball.ui.components.InputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CreateTextFieldOutline(label: String, 
                           value: String, 
                           isSingleLine: Boolean = true,
                           onValueChange: (String) -> Unit = {},
                           isReadOnly: Boolean = false,
                           isRequired: Boolean = false,
                           isError: Boolean = false,
                           errorMessage: String = stringResource(id = R.string.mandatory_field)
) {
    val labelText = if (isRequired) {
        "$label *"
    } else {
        label
    }

    Column {
        Spacer(Modifier.height(4.dp))
        OutlinedTextField(
            label = { Text(text = labelText) },
            value = value,
            onValueChange = onValueChange,
            singleLine = isSingleLine,
            readOnly = isReadOnly,
            isError = isError,
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