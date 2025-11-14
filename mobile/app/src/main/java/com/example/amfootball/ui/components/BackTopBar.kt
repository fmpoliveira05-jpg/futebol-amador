package com.example.amfootball.ui.components

import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.navigation.NavHostController
import com.example.amfootball.ui.components.buttons.BackButton

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun BackTopBar(navHostController: NavHostController,
               title: String) {
    TopAppBar(
        title = { Text(text = title) },
        navigationIcon = {
            BackButton(navController = navHostController)
        }
        //Poderei meter actions
    )
}