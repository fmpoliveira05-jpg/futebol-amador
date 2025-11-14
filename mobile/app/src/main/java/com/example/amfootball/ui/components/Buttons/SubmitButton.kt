package com.example.amfootball.ui.components.Buttons

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Save
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R

@Composable
fun SubmitFormButton(onClick: () -> Unit,
                     imageButton: ImageVector = Icons.Default.Save,
                     text: String = stringResource(id = R.string.submit_button),
                     contentDescription: String = stringResource(id = R.string.submit_button_description)
) {
    Button(
        onClick = onClick,
        modifier = Modifier
            .fillMaxWidth()
            .height(56.dp)
    ) {
        Icon(
            imageVector = imageButton,
            contentDescription = contentDescription
        )
        Spacer(Modifier.width(8.dp))
        Text(text = text)
    }
}