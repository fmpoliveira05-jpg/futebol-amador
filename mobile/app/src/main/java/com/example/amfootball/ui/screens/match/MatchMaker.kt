package com.example.amfootball.ui.screens.match

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Map
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.livedata.observeAsState
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.res.painterResource
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavHostController
import androidx.navigation.compose.rememberNavController
import com.example.amfootball.data.dtos.match.MatchMakerInfo
import com.example.amfootball.data.dtos.team.InfoTeamMatchMaker
import com.example.amfootball.ui.components.Loading
import com.example.amfootball.ui.components.lists.ImageList
import com.example.amfootball.ui.viewModel.match.MatchMakerViewModel
import com.example.amfootball.R

@Composable
fun MatchMakerScreen(
    navHostController: NavHostController,
    viewModel: MatchMakerViewModel = viewModel()
) {
    val infoTeam by viewModel.matchMaker.observeAsState()

    if (infoTeam == null) {
        Loading()
        return
    }

    MatchMakerContent(
        matchMakerInfo = infoTeam!!,
        cancelAction = {
            viewModel.onCancelFind()
        },
        modifier = Modifier
            .fillMaxSize()
            .padding(16.dp)
    )
}

@Composable
private fun MatchMakerContent(
    matchMakerInfo: MatchMakerInfo,
    cancelAction: () -> Unit,
    modifier: Modifier = Modifier,
) {
    Column(
        modifier = modifier,
        verticalArrangement = Arrangement.SpaceAround,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        val pitchModifier = Modifier
            .fillMaxWidth()
            .height(180.dp)
            .clip(RoundedCornerShape(12.dp))
            .background(Color.LightGray)

        if (!matchMakerInfo.pitch.isNullOrBlank()) {
            PichInfo(
                pitch = matchMakerInfo.pitch,
                modifier = pitchModifier
            )
        } else {
            PichInfoPlaceholder(modifier = pitchModifier)
        }


        Spacer(modifier = Modifier.height(32.dp))

        Teams(
            teams = matchMakerInfo.team,
            modifier = Modifier.fillMaxWidth()
        )

        Spacer(modifier = Modifier.height(48.dp))

        CancelButton(
            onClick = { cancelAction() },
            modifier = Modifier
                .fillMaxWidth(0.8f)
                .height(56.dp)
        )
    }
}

@Composable
private fun PichInfo(
    pitch: String = "Campo Desconhecido",
    modifier: Modifier = Modifier
) {
    Box(
        modifier = modifier
    ) {
        Image(
            painter = painterResource(id = R.drawable.map_placeholder),
            contentDescription = "Campo de Jogo",
            contentScale = ContentScale.Crop,
            modifier = Modifier.fillMaxSize()
        )

        Column(
            modifier = Modifier
                .fillMaxSize()
                .background(Color.Black.copy(alpha = 0.4f))
                .padding(16.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                text = "Campo de Jogo",
                style = MaterialTheme.typography.titleLarge,
                color = Color.White
            )
            Text(
                text = pitch,
                style = MaterialTheme.typography.bodyLarge,
                color = Color.White
            )
        }
    }
}

@Composable
private fun PichInfoPlaceholder(
    modifier: Modifier = Modifier
) {
    Box(
        modifier = modifier.background(MaterialTheme.colorScheme.surfaceVariant),
        contentAlignment = Alignment.Center
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            Icon(
                imageVector = Icons.Default.Map,
                contentDescription = "A procurar campo",
                modifier = Modifier.size(48.dp),
                tint = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Text(
                text = stringResource(id = R.string.find_pitch),
                style = MaterialTheme.typography.titleMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

@Composable
private fun Teams(
    teams: List<InfoTeamMatchMaker>,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = modifier,
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.SpaceAround
    ) {
        if (teams.isNotEmpty()) {
            InfoTeam(
                team = teams[0],
                modifier = Modifier.weight(1f),
            )
        } else {
            InfoTeamPlaceholder()
        }

        Text(
            text = "VS",
            style = MaterialTheme.typography.headlineLarge,
            modifier = Modifier.padding(horizontal = 16.dp)
        )

        if (teams.size > 1) {
            InfoTeam(
                team = teams[1],
                modifier = Modifier.weight(1f)
            )
        } else {
            InfoTeamPlaceholder()
        }
    }
}

@Composable
private fun CancelButton(
    onClick: () -> Unit,
    modifier: Modifier = Modifier
) {
    Button(
        onClick = onClick ,
        modifier = modifier,
        colors = ButtonDefaults.buttonColors(
            containerColor = MaterialTheme.colorScheme.error
        ),
        shape = RoundedCornerShape(12.dp)
    ) {
        Text("Cancelar", style = MaterialTheme.typography.titleMedium)
    }
}

@Composable
private fun InfoTeam(
    team: InfoTeamMatchMaker,
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        ImageList(
            image = team.logoTeam
        )
        Surface(
            shape = RoundedCornerShape(8.dp),
            color = MaterialTheme.colorScheme.primaryContainer.copy(alpha = 0.5f)
        ) {
            Row(
                modifier = Modifier.padding(horizontal = 8.dp, vertical = 4.dp),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(4.dp)
            ) {
                Icon(
                    imageVector = Icons.Default.Star,
                    contentDescription = "Rank",
                    modifier = Modifier.size(16.dp)
                )

                Text(
                    text = team.rank,
                    style = MaterialTheme.typography.labelLarge
                )
            }
        }

        Text(team.name, style = MaterialTheme.typography.titleMedium)
    }
}


@Composable
private fun InfoTeamPlaceholder(
    modifier: Modifier = Modifier
) {
    Column(
        modifier = modifier,
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.spacedBy(8.dp)
    ) {
        Icon(
            imageVector = Icons.Default.Person,
            contentDescription = null,
            modifier = Modifier
                .size(100.dp)
                .clip(CircleShape)
                .background(Color.LightGray.copy(alpha = 0.5f))
                .padding(8.dp)
        )

        Spacer(modifier = Modifier.height(4.dp))

        Box(
            modifier = Modifier
                .fillMaxWidth(0.6f)
                .height(24.dp) // Ligeiramente mais alto para o texto "respirar"
                .background(Color.LightGray.copy(alpha = 0.7f), RoundedCornerShape(4.dp)),
            contentAlignment = Alignment.Center
        ) {
            Text(
                text = stringResource(id = R.string.find_opponent),
                style = MaterialTheme.typography.labelLarge,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }

        Spacer(modifier = Modifier.height(8.dp))

        Box(
            modifier = Modifier
                .fillMaxWidth(0.4f)
                .height(16.dp)
                .background(Color.LightGray.copy(alpha = 0.7f), RoundedCornerShape(4.dp))
        )
    }
}

@Preview(
    name = "Procura Automatica de partida - PT",
    locale = "pt",
    showBackground = true
)
@Preview(
    name = "Match Maker - EN",
    locale = "en",
    showBackground = true
)
@Composable
fun MatchMakerPreview() {
    MatchMakerScreen(navHostController = rememberNavController())
}