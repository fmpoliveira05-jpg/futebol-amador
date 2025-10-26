package com.example.amfootball.data

import androidx.compose.ui.graphics.vector.ImageVector

data class NavigationItem(
    val label: String,
    val description: String,
    val icon: ImageVector,
    val route: String
)
