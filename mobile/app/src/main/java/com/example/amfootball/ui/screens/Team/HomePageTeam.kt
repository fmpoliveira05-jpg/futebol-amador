package com.example.amfootball.ui.screens.Team

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Button
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.ui.components.NavBar.NavigatonDrawerTeam

@Composable
fun HomePageTeamScreen(globalNavController: NavHostController){
    Surface(modifier = Modifier.fillMaxSize()) {
        Column {
            //NavigatonDrawerTeam()
            Text(text = "HomePage da equipa")
        }
    }
}

@Preview(showBackground = true)
@Composable
fun PreviewHomePageTeamScreen() {
    HomePageTeamScreen(globalNavController = rememberNavController())
}