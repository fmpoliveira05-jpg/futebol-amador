package com.example.amfootball.ui.screens

import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Button
import androidx.compose.material3.Divider
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.example.amfootball.ui.TeamRow
import com.example.amfootball.viewmodel.MainViewModel


/*
@Composable
fun LeaderboardScreen(onBack: () -> Unit, viewModel: MainViewModel) {
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text("Leaderboard", modifier = Modifier.padding(8.dp))
        val board = viewModel.getLeaderboard()

        LazyColumn {
            items(board) { team ->
                TeamRow(team)
                Divider()
            }
        }

        Spacer(modifier = Modifier.height(8.dp))
        Button(onClick = onBack) { Text("Voltar") }
    }
}
* */
@Composable
fun LeaderboardScreen() {
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text("Leaderboard", modifier = Modifier.padding(8.dp))
    }
}