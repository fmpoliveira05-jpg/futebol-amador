package com.example.amfootball.ui.components.Buttons

import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.width
import androidx.compose.material3.Button
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.graphics.vector.ImageVector


@Composable
fun NavigateButton (icon: ImageVector, label: String, onClick: () -> Unit) {
    Button (onClick = onClick,
        modifier = Modifier.height(56.dp)
    ){
        Icon(imageVector = icon, contentDescription = label)
        Spacer(Modifier.width(8.dp))
        Text(text = label)
    }

}