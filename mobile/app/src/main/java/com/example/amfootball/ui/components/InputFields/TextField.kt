package com.example.amfootball.ui.components.InputFields

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.OutlinedTextField
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CreateTextFieldOutline(label: String, 
                           value: String, 
                           isSingleLine: Boolean = true,
                           onValueChange: (String) -> Unit = {},
                           isReadOnly: Boolean = false
) {
    Column {
        Spacer(Modifier.height(4.dp))
        OutlinedTextField(
            label = { Text(text = label) },
            value = value,
            onValueChange = onValueChange,
            singleLine = isSingleLine,
            readOnly = isReadOnly,
        )
    }
}