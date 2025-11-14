package com.example.amfootball.ui.components.inputFields

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp

@Composable
fun Label(label: String,
          isRequired: Boolean) {
    val labelText = formatRequiredLabel(label = label, isRequired = isRequired)

    Text(
        text = labelText,
        style = MaterialTheme.typography.bodyMedium,
        modifier = Modifier
            .padding(top = 16.dp)
    )
}


fun formatRequiredLabel(label: String, isRequired: Boolean): String {
    var labelText = label

    if (isRequired) {
        labelText = "$label *"
    }

    return labelText
}