package com.example.amfootball.ui.screens

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.example.amfootball.data.User

@Composable
//fun HomePageScreen(currentUser: User?){
fun HomePageScreen() {
    Column {
        //NavigatonDrawerNavBarHomePage()
        Text(text = "AMFootball", modifier = Modifier.padding(8.dp))
        //Text(text = "Bem-vindo ${currentUser?.name ?: "Convidado"}", modifier = Modifier.padding(8.dp))
    }
}