package com.example.amfootball.ui.screens.Team.PostPoneMatch

import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.ui.components.BackTopBar

//Isto é como um MatchInvite, eu posso simplesmente fazer como o Create e UpdateTeam
//Tem de também aparecer aqui os dados das partiaas
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun PostPoneMatchScreen(navHostController: NavHostController) {
    Scaffold(
        topBar = {
            BackTopBar(
                navHostController = navHostController,
                title = "Adiar Partida",
            )
        },
        content = { paddingValues ->
            ContentPostPoneMatch(modifier = Modifier
                .padding(paddingValues)
                .padding(16.dp),
                navHostController = navHostController)
        }
    )
}

@Composable
private fun ContentPostPoneMatch(modifier: Modifier = Modifier,
                                 navHostController: NavHostController) {

}

@Preview(showBackground = true)
@Composable
fun PreviewPostPoneMatch(){
    PostPoneMatchScreen(rememberNavController())
}