package com.example.amfootball.ui.screens

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.navigation.objects.Routes

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
                globalNavController.navigate(route = Routes.TeamRoutes.CREATE_TEAM.route)
            }
        ) {
            Text(text = "Criar equipa")
        }

        Button(
            onClick = {
                globalNavController.navigate(
                    route = "${Routes.TeamRoutes.UPDATE_TEAM.route}/CC2B558C-F362-4B40-B05A-79E8EABA2F8F")
            }
        ) {
            Text(text = "Editar equipa")
        }

        Button(
            onClick = {
                globalNavController.navigate(
                    route = "${Routes.TeamRoutes.MEMBERLIST.route}/105ABBA3-76FF-4499-A6BD-44179287E563")
            }
        ) {
            Text(text = "Lista de membros")
        }

    }
}


@Preview(showBackground = true)
@Composable
fun PreviewHomePage() {
    HomePageScreen(globalNavController = rememberNavController())
}