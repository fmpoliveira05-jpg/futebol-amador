package com.example.amfootball.ui.screens

import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import com.example.amfootball.ui.viewModel.LeadBoardViewModel
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.Icon
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.dtos.leadboardDto.LeadboardDto


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

Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Text("Leaderboard", modifier = Modifier.padding(8.dp))
    }
* */
@Composable
fun LeaderboardScreen(
    navHostController: NavHostController,
    viewModel: LeadBoardViewModel = viewModel()
) {
    val list = viewModel.inicialList.value

    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            //METER UM ITEM PARA AS TEAMS
            items(list) { team ->
                LeaderBoardContent(
                    team = team,
                    showMoreAction = { viewModel.loadMoreTeams() },
                    showInforTeam = { viewModel.showInfoTeam(
                        idTeam = team.Team.id,
                        navHostController = navHostController
                    ) }
                )
            }
        }
    }
}

@Composable
private fun LeaderBoardContent(
    team: LeadboardDto,
    showMoreAction: () -> Unit,
    showInforTeam: () -> Unit
) {
    ListItem(
        headlineContent = {
            Text(text = team.Team.name)
        },
        supportingContent = {
            Text(
                text = buildAnnotatedString {
                    pushStyle(SpanStyle(fontWeight = FontWeight.Bold))
                    append("Rank: ${team.Team.nameRank}")
                    pop()

                    append("  ")

                    pushStyle(SpanStyle(color = MaterialTheme.colorScheme.primary))
                    append("(${team.Team.currentPoints} Pts)")
                    pop()
                },
                style = MaterialTheme.typography.bodyMedium,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        },
        leadingContent = { // imagem
            Icon(
                imageVector = Icons.Default.Star,
                contentDescription = "Logo Team",
                tint = MaterialTheme.colorScheme.primary,
                modifier = Modifier.size(40.dp)
            )
        },

    )
}

@Preview(
    name = "LeadBoard - PT",
    locale = "pt-rPT",
    showBackground = true
)
@Preview(
    name = "LeadBoard - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun LeaderboardScreenPreview() {
    LeaderboardScreen(rememberNavController())
}