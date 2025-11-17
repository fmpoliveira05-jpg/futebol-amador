package com.example.amfootball.ui.screens.settings

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.navigation.NavController
import com.example.amfootball.ui.components.buttons.NavigateButton
import com.example.amfootball.ui.theme.AMFootballTheme

@Composable
fun SettingsScreen(
    modifier: Modifier = Modifier,
    navController: NavController
){
    var darkMode = false

    LazyColumn() {
        item {
            Button(
                modifier = modifier,
                onClick =
                {

                })
            {
                DarkMode()
            }
        }
        item {
            //NavigateButton(modifier, ) { }
        }
    }
}

@Composable
fun DarkMode() {
    AMFootballTheme(darkTheme = false, dynamicColor = false) {
    }
}
