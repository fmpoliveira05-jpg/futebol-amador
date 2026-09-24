package com.example.amfootball.ui.components.pages.homePage

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.ExitToApp
import androidx.compose.material3.Card
import androidx.compose.material3.CardDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.example.amfootball.R
import com.example.amfootball.core.utils.Patterns
import com.example.amfootball.data.remote.dtos.homePageTeam.VitorySequenceTemDto
import com.example.amfootball.data.remote.dtos.match.InfoMatch
import com.example.amfootball.domains.enums.match.MatchResult
import com.example.amfootball.ui.components.actionCards.ActionCard
import java.time.format.DateTimeFormatter

/**
 * Componente Composable que exibe a secção de "Forma Recente" de uma equipa.
 *
 * Mostra os resultados dos últimos jogos da equipa (limitado aos 5 mais recentes, se existirem).
 *
 * @param history A lista de DTOs ([VitorySequenceTemDto]) contendo o resultado e o adversário de jogos recentes.
 */
@Composable
fun RecentFormSection(history: List<VitorySequenceTemDto>) {
    val maxItems = 5
    val displayHistory = history.take(maxItems)
    val emptySlots = maxItems - displayHistory.size

    Column(modifier = Modifier.fillMaxWidth()) {
        Text(
            text = stringResource(id = R.string.team_form),
            style = MaterialTheme.typography.titleMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.padding(bottom = 8.dp)
        )

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(4.dp)
        ) {
            if (history.isEmpty()) {
                Text(
                    stringResource(id = R.string.no_game_realized),
                    style = MaterialTheme.typography.bodyMedium
                )
            } else {
                displayHistory.forEach { match ->
                    MatchResultBadge(
                        match = match,
                        modifier = Modifier.weight(1f)
                    )
                }

                repeat(emptySlots) {
                    Spacer(modifier = Modifier.weight(1f))
                }
            }
        }
    }
}

/**
 * Componente Composable que exibe o resultado de um único jogo numa insígnia circular colorida.
 *
 * A cor do círculo é determinada pelo [MatchResult]: Verde para vitória, Vermelho para derrota, Amarelo para empate.
 *
 * @param match O DTO [VitorySequenceTemDto] com o resultado e o nome do adversário.
 */
@Composable
fun MatchResultBadge(
    match: VitorySequenceTemDto,
    modifier: Modifier = Modifier
) {
    val color = when (match.matchResult) {
        MatchResult.WIN -> Color(0xFF4CAF50)
        MatchResult.LOSE -> Color(0xFFE53935)
        MatchResult.DRAW -> Color(0xFFFFC107)
        else -> Color.Gray
    }

    Column(
        horizontalAlignment = Alignment.CenterHorizontally,
        modifier = modifier
    ) {
        Box(
            modifier = Modifier
                .size(40.dp)
                .clip(CircleShape)
                .background(color),
            contentAlignment = Alignment.Center
        ) {
            Text(
                text = match.result,
                color = Color.White,
                style = MaterialTheme.typography.labelSmall,
                fontWeight = FontWeight.Bold
            )
        }
        Spacer(modifier = Modifier.height(4.dp))

        Text(
            text = match.opponent.name,
            style = MaterialTheme.typography.labelSmall,
            maxLines = 1,
            overflow = TextOverflow.Ellipsis,
            fontSize = 10.sp
        )
    }
}

/**
 * Componente Composable que exibe a secção de "Próximos Jogos" da equipa.
 *
 * Mostra uma linha horizontal de [UpcomingMatchCard] para os 3 jogos agendados mais próximos.
 *
 * @param matches A lista de DTOs [InfoMatch] dos jogos futuros.
 */
@Composable
fun UpcomingMatchesSection(matches: List<InfoMatch>) {
    Column(modifier = Modifier.fillMaxWidth()) {
        Text(
            text = stringResource(id = R.string.next_games),
            style = MaterialTheme.typography.titleMedium,
            fontWeight = FontWeight.Bold,
            modifier = Modifier.padding(vertical = 8.dp)
        )

        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            if (matches.isEmpty()) {
                Text(
                    stringResource(id = R.string.no_games_scheduled),
                    style = MaterialTheme.typography.bodyMedium
                )
            } else {
                matches.take(3).forEach { match ->
                    UpcomingMatchCard(
                        match = match,
                        modifier = Modifier.weight(1f)
                    )
                }
            }
        }
    }
}

/**
 * Cartão de informação simplificado para um próximo jogo.
 *
 * Exibe a data, hora, adversário e se o jogo é "Em Casa" (C) ou "Fora" (F).
 *
 * @param match O DTO [InfoMatch] contendo os detalhes do jogo agendado.
 * @param modifier Modificador de layout.
 */
@Composable
fun UpcomingMatchCard(
    match: InfoMatch,
    modifier: Modifier = Modifier
) {
    val dateFormatter = DateTimeFormatter.ofPattern(Patterns.DATE)
    val timeFormatter = DateTimeFormatter.ofPattern(Patterns.TIME)

    Card(
        modifier = modifier,
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        ),
        shape = RoundedCornerShape(8.dp)
    ) {
        Column(
            modifier = Modifier
                .padding(8.dp)
                .fillMaxWidth(),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Text(
                text = match.gameDate.format(dateFormatter),
                style = MaterialTheme.typography.labelMedium,
                fontWeight = FontWeight.Bold,
                color = MaterialTheme.colorScheme.primary
            )

            Spacer(modifier = Modifier.height(4.dp))

            Text(
                text = match.opponent.name,
                style = MaterialTheme.typography.bodySmall,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis,
                fontWeight = FontWeight.Bold
            )

            Spacer(modifier = Modifier.height(4.dp))

            Row(verticalAlignment = Alignment.CenterVertically) {
                Text(
                    text = match.gameDate.format(timeFormatter),
                    style = MaterialTheme.typography.labelSmall
                )
                Spacer(modifier = Modifier.width(4.dp))
                Text(
                    text = if (match.isHome) "(${stringResource(id = R.string.game_Home)})" else "(${stringResource(id = R.string.game_away)})",
                    style = MaterialTheme.typography.labelSmall,
                    color = if (match.isHome) Color.Gray else Color.Gray
                )
            }
        }
    }
}

/**
 * Cartão de Ação (Action Card) estilizado para a operação de "Sair da Equipa".
 *
 * Este componente utiliza o [ActionCard] genérico para criar uma ação destrutiva
 * que normalmente é exibida na parte inferior da página de gestão da equipa.
 *
 * @param onClick Callback executada ao clicar no cartão.
 */
@Composable
fun ActionCardLeaveTeam(onClick: () -> Unit) {
    ActionCard(
        title = stringResource(id = R.string.action_card_leave_team),
        subtitle = stringResource(id = R.string.action_card_leave_team_description),
        icon = Icons.AutoMirrored.Filled.ExitToApp,
        onClick = onClick,
    )
}