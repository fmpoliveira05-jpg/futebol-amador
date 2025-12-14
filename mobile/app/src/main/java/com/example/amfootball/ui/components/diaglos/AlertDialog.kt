package com.example.amfootball.ui.components.diaglos

import androidx.compose.material3.AlertDialog
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable

@Composable
fun AppAlertDialog(
    title: String,
    text: String,
    confirmButtonText: String,
    dismissButtonText: String,
    onConfirmClick: () -> Unit,
    onDismissClick: () -> Unit = {}
) {
    AlertDialog(
        onDismissRequest = onDismissClick,
        title = {
            Text(text = title)
        },
        text = {
            Text(text = text)
        },
        confirmButton = {
            TextButton(
                onClick = {
                    onConfirmClick()
                    onDismissClick()
                }
            ) {
                Text(
                    text = confirmButtonText,
                    color = MaterialTheme.colorScheme.error
                )
            }
        },
        dismissButton = {
            TextButton(
                onClick = onDismissClick
            ) {
                Text(text = dismissButtonText)
            }
        }
    )
}