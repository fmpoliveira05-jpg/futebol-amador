package com.example.amfootball.ui.screens

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.R
import com.example.amfootball.data.User
import com.example.amfootball.navigation.Objects.Pages.CrudTeamRoutes
import com.example.amfootball.navigation.Objects.Pages.MatchInviteRoutes

@Composable
//fun HomePageScreen(currentUser: User?){
fun HomePageScreen(globalNavController: NavHostController) {
    Column {
        //NavigatonDrawerNavBarHomePage()
        Text(text = "AMFootball", modifier = Modifier.padding(8.dp))
        //Text(text = "Bem-vindo ${currentUser?.name ?: "Convidado"}", modifier = Modifier.padding(8.dp))
        Text(text = "Bem-vindo utilizador")
        Button(
            onClick = {
                globalNavController.navigate(route = CrudTeamRoutes.CREATE_TEAM)
            }
        ) {
            Text(text = "Criar equipa")
        }
    }
}


@Preview(showBackground = true)
@Composable
fun PreviewHomePage() {
    HomePageScreen(globalNavController = rememberNavController())
}