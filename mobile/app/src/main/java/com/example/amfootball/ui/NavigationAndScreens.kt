package com.example.amfootball.ui

import android.Manifest
import android.location.Location
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.material3.Button
import androidx.compose.ui.unit.dp
import androidx.compose.material3.Divider
import com.example.amfootball.data.Match
import com.example.amfootball.data.Team
import com.example.amfootball.data.User
import com.example.amfootball.viewmodel.MainViewModel
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import java.time.ZoneId
import java.time.format.DateTimeFormatter

@Composable
fun AMFootballApp(viewModel: MainViewModel = viewModel()) {
    // Simple state: current user (demo: first user or guest)
    val users by viewModel.users.collectAsState()
    var currentUser by remember { mutableStateOf<User?>(users.firstOrNull()) }

    // UI scaffold minimal: show screens with simple nav
    var currentScreen by remember { mutableStateOf("home") }
    when (currentScreen) {
        "home" -> HomeScreen(
            currentUser = currentUser,
            onNavigate = { currentScreen = it },
            viewModel = viewModel
        )
        "leaderboard" -> LeaderboardScreen(onBack = { currentScreen = "home" }, viewModel = viewModel)
        "teams" -> TeamsScreen(onBack = { currentScreen = "home" }, viewModel = viewModel)
        "search" -> SearchMatchesScreen(onBack = { currentScreen = "home" }, viewModel = viewModel, currentUser)
        "map" -> MapScreenContainer(onBack = { currentScreen = "home" }, viewModel = viewModel)
    }
}

@Composable
fun HomeScreen(currentUser: User?, onNavigate: (String) -> Unit, viewModel: MainViewModel) {
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text(text = "AMFootball", modifier = Modifier.padding(8.dp))
        Text(text = "Bem-vindo ${currentUser?.name ?: "Convidado"}", modifier = Modifier.padding(8.dp))
        Spacer(modifier = Modifier.height(8.dp))
        Button(onClick = { onNavigate("leaderboard") }, modifier = Modifier.fillMaxWidth()) { Text("Leaderboard") }
        Spacer(modifier = Modifier.height(8.dp))
        Button(onClick = { onNavigate("teams") }, modifier = Modifier.fillMaxWidth()) { Text("Equipas") }
        Spacer(modifier = Modifier.height(8.dp))
        Button(onClick = { onNavigate("search") }, modifier = Modifier.fillMaxWidth()) { Text("Procurar partidas ranqueadas semelhantes") }
        Spacer(modifier = Modifier.height(8.dp))
        Button(onClick = { onNavigate("map") }, modifier = Modifier.fillMaxWidth()) { Text("Mapa / Partidas próximas") }
    }
}

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

@Composable
fun TeamRow(team: Team) {
    Row(modifier = Modifier.fillMaxWidth().padding(12.dp), horizontalArrangement = Arrangement.SpaceBetween) {
        Text(team.name)
        Text("Rating: ${team.rating}")
    }
}

@Composable
fun TeamsScreen(onBack: () -> Unit, viewModel: MainViewModel) {
    val teams by viewModel.teams.collectAsState()
    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text("Equipas", modifier = Modifier.padding(8.dp))
        LazyColumn {
            items(teams) { t ->
                TeamRow(t)
                Divider()
            }
        }
        Spacer(modifier = Modifier.height(8.dp))
        Button(onClick = onBack) { Text("Voltar") }
    }
}

@Composable
fun SearchMatchesScreen(onBack: () -> Unit, viewModel: MainViewModel, currentUser: User?) {
    val teams by viewModel.teams.collectAsState()
    var selectedTeamId by remember { mutableStateOf<String?>(currentUser?.teamId ?: teams.firstOrNull()?.id) }
    var threshold by remember { mutableStateOf(100) }
    var results by remember { mutableStateOf<List<Pair<com.example.amfootball.data.Team, com.example.amfootball.data.Team>>>(emptyList()) }

    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text("Procurar partidas ranqueadas", modifier = Modifier.padding(8.dp))
        // Simple team selector
        LazyColumn(modifier = Modifier.weight(1f)) {
            items(teams) { t ->
                Row(modifier = Modifier.fillMaxWidth().padding(8.dp).clickable { selectedTeamId = t.id }) {
                    Text(t.name)
                    Spacer(modifier = Modifier.weight(1f))
                    Text("R: ${t.rating}")
                }
            }
        }
        Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
            Button(onClick = {
                selectedTeamId?.let {
                    results = viewModel.findRankedMatchesForTeam(it, threshold)
                }
            }) { Text("Procurar (threshold $threshold)") }
            Button(onClick = onBack) { Text("Voltar") }
        }
        Spacer(modifier = Modifier.height(8.dp))
        LazyColumn {
            items(results) { pair ->
                Column(modifier = Modifier.padding(8.dp)) {
                    Text("${pair.first.name} vs ${pair.second.name}")
                    Text("Ratings: ${pair.first.rating} - ${pair.second.rating}")
                }
                Divider()
            }
        }
    }
}