package com.example.amfootball.ui.components.lists

import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.width
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Badge
import androidx.compose.material.icons.filled.CalendarMonth
import androidx.compose.material.icons.filled.CalendarToday
import androidx.compose.material.icons.filled.Groups
import androidx.compose.material.icons.filled.LocationOn
import androidx.compose.material.icons.filled.SportsSoccer
import androidx.compose.material.icons.filled.Stadium
import androidx.compose.material.icons.filled.Straighten
import androidx.compose.material3.Icon
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.res.stringResource
import androidx.compose.ui.unit.dp
import com.example.amfootball.R
import com.example.amfootball.data.enums.Position
import com.example.amfootball.data.enums.TypeMember
import com.example.amfootball.utils.TeamConst
import com.example.amfootball.utils.extensions.toOneDecimal

/**
 * Componente base que exibe um ícone e um texto numa linha horizontal.
 *
 * O ícone e o texto são estilizados com a cor de contorno ([MaterialTheme.colorScheme.outline])
 * para um visual discreto. O tamanho do ícone é fixo em 16.dp.
 *
 * @param icon O [ImageVector] do ícone a ser exibido.
 * @param contentDescription Descrição de conteúdo para acessibilidade do ícone.
 * @param text A string de texto a ser exibida.
 * @param modifier Modificador para estilizar o [Row].
 */
@Composable
fun InfoRow(
    icon: ImageVector,
    contentDescription: String? = null,
    text: String,
    modifier: Modifier = Modifier
) {
    Row(
        verticalAlignment = Alignment.CenterVertically,
        modifier = modifier
    ) {
        Icon(
            imageVector = icon,
            contentDescription = contentDescription,
            modifier = Modifier.size(16.dp),
            tint = MaterialTheme.colorScheme.outline
        )
        Spacer(Modifier.width(4.dp))
        Text(
            text = text,
            style = MaterialTheme.typography.bodySmall,
            color = MaterialTheme.colorScheme.outline
        )
    }
}

/**
 * Exibe uma data formatada com o ícone de calendário.
 * @param date A string da data a ser exibida.
 */
@Composable
fun DateRow(date: String) {
    InfoRow(
        icon = Icons.Default.CalendarMonth,
        text = date,
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Exibe a idade de uma pessoa com o ícone de calendário.
 * @param age A idade (Int) a ser exibida. Adiciona a string "anos" do recurso.
 */
@Composable
fun AgeRow(age: Int) {
    InfoRow(
        icon = Icons.Default.CalendarToday,
        text = age.toString() + stringResource(id = R.string.age),
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Componente UI que exibe uma linha informativa com a Idade Média.
 *
 * Utiliza o componente genérico [InfoRow] com um ícone de calendário.
 * O valor da idade é formatado automaticamente para apresentar apenas uma casa decimal.
 *
 * @param age O valor numérico (Double) da idade média a ser exibido (ex: 22.543 -> 22.5).
 */
@Composable
fun AverageAgeRow(age: Double) {
    val formattedAge = age.toOneDecimal()
    val label = stringResource(id = R.string.average_age)

    InfoRow(
        icon = Icons.Default.CalendarToday,
        text = "$formattedAge $label",
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Exibe um endereço de localização com o ícone de pino.
 * @param address A string do endereço.
 */
@Composable
fun AddressRow(address: String) {
    InfoRow(
        icon = Icons.Default.LocationOn,
        text = address,
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Exibe o tamanho (altura) com o ícone de régua.
 * @param heigth O tamanho (Int) em centímetros.
 */
@Composable
fun SizeRow(height: Int) {
    InfoRow(
        icon = Icons.Default.Straighten,
        text = "$height cm",
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Exibe a posição de jogo de um jogador.
 * @param position A posição do jogador (Enum [Position]). O texto é obtido via [stringResource].
 */
@Composable
fun PositionRow(position: Position) {
    InfoRow(
        icon = Icons.Default.SportsSoccer,
        text = stringResource(id = position.stringId),
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Exibe o tipo de membro da equipa (ex: Treinador, Jogador).
 * @param typeMember O tipo de membro (Enum [TypeMember]). O texto é obtido via [stringResource].
 */
@Composable
fun TypeMemberRow(typeMember: TypeMember) {
    InfoRow(
        icon = Icons.Default.Badge,
        text = stringResource(id = typeMember.stringId),
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Exibe o endereço do campo de jogo ou estádio.
 * @param ptichAdrress O endereço do campo.
 */
@Composable
fun PitchAddressRow(ptichAdrress: String) {
    InfoRow(
        icon = Icons.Default.Stadium,
        text = ptichAdrress,
        modifier = Modifier.fillMaxWidth()
    )
}

/**
 * Exibe o número atual de membros de uma equipa comparado ao limite máximo.
 * @param numMembers O número atual de membros. O limite máximo é obtido de [TeamConst.MAX_MEMBERS].
 */
@Composable
fun NumMembersTeamRow(numMembers: Int) {
    InfoRow(
        icon = Icons.Default.Groups,
        text = "$numMembers/${TeamConst.MAX_MEMBERS} ${stringResource(id = R.string.list_teams_members)}",
        modifier = Modifier.fillMaxWidth()
    )
}