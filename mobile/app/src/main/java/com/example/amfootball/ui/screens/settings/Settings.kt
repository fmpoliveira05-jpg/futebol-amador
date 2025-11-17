package com.example.amfootball.ui.screens.settings

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.NavController

@Composable
fun SettingsScreen(
    modifier: Modifier = Modifier,
    navController: NavController
){
    LazyColumn() {
        item {
            Text(text = "Settings")
        }
    }
}
