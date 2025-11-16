package com.example.amfootball.ui.screens

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
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
import androidx.compose.material3.FilledTonalButton
import androidx.compose.material3.Icon
import androidx.compose.material3.ListItem
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.ui.Modifier
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.SpanStyle
import androidx.compose.ui.text.buildAnnotatedString
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.dtos.leadboard.LeadboardDto
import com.example.amfootball.ui.components.buttons.ShowMoreInfoButton
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.R

@Composable
fun LeaderboardScreen(
    navHostController: NavHostController,
    viewModel: LeadBoardViewModel = viewModel()
) {
    val list = viewModel.inicialList.value
    val showButton = viewModel.showMoreButton.value

    Surface {
        LazyColumn(
            contentPadding = PaddingValues(horizontal = 16.dp, vertical = 8.dp)
        ) {
            item {
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.Center
                ) {
                    Text(
                        text = stringResource(id = R.string.item_leadboard),
                        style = MaterialTheme.typography.headlineSmall // Estilo de título
                    )
                }
            }

            items(list) { team ->
                LeaderBoardContent(
                    team = team,
                    showInfoTeam = { viewModel.showInfoTeam(
                        idTeam = team.Team.id,
                        navHostController = navHostController)
                    }
                )
            }

            if (showButton) {
                item {
                    FilledTonalButton(
                        onClick = { viewModel.loadMoreTeams() },
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text(
                            text = stringResource(id = R.string.button_view_more),
                            style = MaterialTheme.typography.bodyMedium,
                            maxLines = 1
                        )
                    }
                }
            }
        }
    }
}

@Composable
private fun LeaderBoardContent(
    team: LeadboardDto,
    showInfoTeam: () -> Unit
) {
    ListItem(
        overlineContent = {
            Text(
                text = "#${team.Position}",
                style = MaterialTheme.typography.titleSmall,
                fontWeight = FontWeight.Bold
            )
        },
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
        leadingContent = {
            ImageList(
                image = team.Team.logoTeam
            )
        },
        trailingContent = {
            ShowMoreInfoButton(
                showMoreDetails = { showInfoTeam() }
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